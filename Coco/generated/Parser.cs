/*----------------------------------------------------------------------
Compiler Generator Coco/R,
Copyright (c) 1990, 2004 Hanspeter Moessenboeck, University of Linz
extended by M. Loeberbauer & A. Woess, Univ. of Linz
with improvements by Pat Terry, Rhodes University

This program is free software; you can redistribute it and/or modify it 
under the terms of the GNU General Public License as published by the 
Free Software Foundation; either version 2, or (at your option) any 
later version.

This program is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License 
for more details.

You should have received a copy of the GNU General Public License along 
with this program; if not, write to the Free Software Foundation, Inc., 
59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.

As an exception, it is allowed to write an extension of Coco/R that is
used as a plugin in non-free software.

If not otherwise stated, any source code generated by Coco/R (other than 
Coco/R itself) does not fall under the GNU General Public License.
-----------------------------------------------------------------------*/

using System;
using System.Collections;

namespace Tastier {



public class Parser {
	public const int _EOF = 0;
	public const int _number = 1;
	public const int _ident = 2;
	public const int _string = 3;
	public const int maxT = 49;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;

	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

const int // object kinds
      var = 0, proc = 1, constant = 3, array = 4;

   const int // types
      undef = 0, integer = 1, boolean = 2;

   public SymbolTable tab;
   public CodeGenerator gen;

/*-------------------------------------------------------------------------------------------*/



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}

	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}

	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}

	bool StartOf (int s) {
		return set[s, la.kind];
	}

	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}


	void AddOp(out Op op) {
		op = Op.ADD; 
		if (la.kind == 4) {
			Get();
		} else if (la.kind == 5) {
			Get();
			op = Op.SUB; 
		} else SynErr(50);
	}

	void Expr(out int reg,        // load value of Expr into register
out int type) {
		int typeR, regR; Op op; 
		SimExpr(out reg,
out type);
		if (StartOf(1)) {
			RelOp(out op);
			SimExpr(out regR,
out typeR);
			if (type == typeR) {
			  type = boolean;
			  gen.RelOp(op, reg, regR);
			}
			else SemErr("incompatible types");
			
		}
		gen.ClearRegisters(); 
	}

	void SimExpr(out int reg,     //load value of SimExpr into register
out int type) {
		int typeR, regR; Op op; 
		Term(out reg,
out type);
		while (la.kind == 4 || la.kind == 5) {
			AddOp(out op);
			Term(out regR,
out typeR);
			if (type == integer && typeR == integer)
			  gen.AddOp(op, reg, regR);
			else SemErr("integer type expected");
			
		}
	}

	void RelOp(out Op op) {
		op = Op.EQU; 
		switch (la.kind) {
		case 21: {
			Get();
			break;
		}
		case 22: {
			Get();
			op = Op.LSS; 
			break;
		}
		case 23: {
			Get();
			op = Op.GTR; 
			break;
		}
		case 24: {
			Get();
			op = Op.NEQ; 
			break;
		}
		case 25: {
			Get();
			op = Op.LEQ; 
			break;
		}
		case 26: {
			Get();
			op = Op.GEQ; 
			break;
		}
		default: SynErr(51); break;
		}
	}

	void Primary(out int reg,     // load Primary into register
out int type) {
		int n, ix, offset; Obj obj; string name; 
		type = undef;
		reg = gen.GetRegister();
		
		switch (la.kind) {
		case 2: {
			Ident(out name);
			if (StartOf(2)) {
				obj = tab.Find(name); type = obj.type;
				if (obj.kind == var | obj.kind == constant)
				{
				
				 //check if the variable being referenced is a parameter.
				 if(obj.isParam == true)
				 {
				
				 //get parameters offset
				   offset = obj.offset + 1 ;
				 //load the value of the parameter into reg
				   gen.loadParam(reg,offset,name);
				 }
				 else
				 {
				
				    if (obj.level == 0)
				       gen.LoadGlobal(reg, obj.adr, name);
				    else
				       gen.LoadLocal(reg, tab.curLevel-obj.level, obj.adr, name);
				
				    if (type == boolean)
				       gen.ResetZ(reg);
				   }
				
				}
				else SemErr("variable expected");
				
			} else if (la.kind == 6) {
				Get();
				Expect(1);
				ix = Convert.ToInt32(t.val); 
				obj = tab.Find(name); type = obj.type;
				
				//check if element that is being accessed is within bounds of the array.
				if(ix >= obj.length) SemErr(" array out of bounds");
				
				if (obj.kind == array) {
				
				//check if the variable being referenced is a parameter.
				if(obj.isParam == true)
				{
				//get parameters offset
				 offset = obj.offset + 1 ;
				
				//load adr into R2.
				 gen.loadParamAdr(offset);
				//load the value of the adr + ix into reg
				 gen.LoadIndexedGlobalValue(reg,ix,name);
				}
				
				
				else
				{
				
				      if (obj.level == 0)
				         gen.LoadIndexedGlobal(reg, obj.adr,ix, name);
				      else
				         gen.LoadIndexedLocal(reg, tab.curLevel-obj.level, obj.adr,ix, name);
				      if (type == boolean)
				         gen.ResetZ(reg);
				}
				
				  }
				else SemErr("array expected");
				
				Expect(7);
			} else SynErr(52);
			break;
		}
		case 1: {
			Get();
			type = integer;
			n = Convert.ToInt32(t.val);
			gen.LoadConstant(reg, n);
			
			break;
		}
		case 5: {
			Get();
			Primary(out reg,
out type);
			if (type == integer)
			  gen.NegateValue(reg);
			else SemErr("integer type expected");
			
			break;
		}
		case 8: {
			Get();
			type = boolean;
			gen.LoadTrue(reg);
			
			break;
		}
		case 9: {
			Get();
			type = boolean;
			gen.LoadFalse(reg);
			
			break;
		}
		case 10: {
			Get();
			Expr(out reg,
out type);
			Expect(11);
			break;
		}
		default: SynErr(53); break;
		}
	}

	void Ident(out string name) {
		Expect(2);
		name = t.val; 
	}

	void String(out string text) {
		Expect(3);
		text = t.val; 
	}

	void MulOp(out Op op) {
		op = Op.MUL; 
		if (la.kind == 12) {
			Get();
		} else if (la.kind == 13 || la.kind == 14) {
			if (la.kind == 13) {
				Get();
			} else {
				Get();
			}
			op = Op.DIV; 
		} else if (la.kind == 15 || la.kind == 16) {
			if (la.kind == 15) {
				Get();
			} else {
				Get();
			}
			op = Op.MOD; 
		} else SynErr(54);
	}

	void ProcDecl(string progName) {
		Obj obj,paramObj; string procName; string name; string str;int type; object temp; ArrayList p = new ArrayList();int count =0;  
		Expect(17);
		Ident(out procName);
		obj = tab.NewObj(procName, proc, undef);
		if (procName == "main")
		  if (tab.curLevel == 0)
		     tab.mainPresent = true;
		  else SemErr("main not at lexic level 0");
		tab.OpenScope();
		
		Expect(10);
		while (la.kind == 44 || la.kind == 45) {
			Type(out type);
			Ident(out name);
			paramObj = tab.NewObj(name,var,type);
			
			//increase parameter count
			count = count + 1;
			
			//add the parameter into list associated with its function
			obj.paramList.Add(name);
			obj.typeList.Add(type);
			p.Add(name);
			
			//object is a parameter
			paramObj.isParam = true;
			
			Expect(18);
		}
		Expect(11);
		obj.paramCount = count;
		
		//reverse the ordering of the params in the stack, since the first param pushed first.
		//First In Last Out
		obj.paramList.Reverse();
		obj.typeList.Reverse();
		p.Reverse();
		
		//adjust the offset of each parameter, the index becomes the offset
		for(int i=0;i<p.Count;i++){
		 temp = p[i];
		 str = Convert.ToString(temp);
		 obj = tab.Find(str);
		 obj.offset = i;
		}
		
		Expect(19);
		while (StartOf(3)) {
			if (la.kind == 44 || la.kind == 45) {
				VarDecl();
			} else if (la.kind == 48) {
				ArrayDecl();
			} else {
				ConstDef();
			}
		}
		while (la.kind == 17) {
			ProcDecl(progName);
		}
		if (procName == "main")
		  gen.Label("Main", "Body");
		else {
		  gen.ProcNameComment(procName);
		  gen.Label(procName, "Body");
		}
		
		Stat();
		while (StartOf(4)) {
			Stat();
		}
		Expect(20);
		if (procName == "main") {
		  gen.StopProgram(progName);
		  gen.Enter("Main", tab.curLevel, tab.topScope.nextAdr);
		} else {
		  gen.Return(procName);
		  gen.Enter(procName, tab.curLevel, tab.topScope.nextAdr);
		}
		tab.CloseScope();
		
	}

	void Type(out int type) {
		type = undef; 
		if (la.kind == 44) {
			Get();
			type = integer; 
		} else if (la.kind == 45) {
			Get();
			type = boolean; 
		} else SynErr(55);
	}

	void VarDecl() {
		string name; int type; 
		Type(out type);
		Ident(out name);
		tab.NewObj(name, var, type); 
		while (la.kind == 46) {
			Get();
			Ident(out name);
			tab.NewObj(name, var, type); 
		}
		Expect(18);
	}

	void ArrayDecl() {
		string name; int type, len; 
		Expect(48);
		Type(out type);
		if(type != integer && type != boolean)  SemErr("array contents can only be of type int or boolean");    
		Ident(out name);
		Expect(6);
		Expect(1);
		len = Convert.ToInt32(t.val); 
		Expect(7);
		Expect(18);
		tab.NewObj(name, array, type);
		
		//allocate space for the elements of the array in the symbol table
		tab.allocateArray(name,len);
		
	}

	void ConstDef() {
		string name; int type, reg; Obj obj; 
		Expect(47);
		Type(out type);
		Ident(out name);
		obj = tab.NewObj(name, constant, type); 
		Expect(30);
		if (obj.kind != constant)
		  SemErr("cannot assign to non constant");
		
		Expr(out reg,
out type);
		Expect(18);
		if (type == obj.type)
		  if (obj.level == 0)
		     gen.StoreGlobal(reg, obj.adr, name);
		
		   else gen.StoreLocal(reg, tab.curLevel-obj.level, obj.adr, name);
		
		// else SemErr("can only define constants globally");
		
		
	}

	void Stat() {
		int type, type1, type2,adrReg,offset,first, ix, cnt; string name,name2; Obj obj, param; int reg, reg1, reg2;object typeObj;int numofParams =0;
		switch (la.kind) {
		case 2: {
			Ident(out name);
			obj = tab.Find(name); 
			if (la.kind == 6) {
				Get();
				Expect(1);
				ix = Convert.ToInt32(t.val);  
				if(ix >= obj.length) SemErr(" array out of bounds"); 
				Expect(7);
				Expect(27);
				Expr(out reg, out type);
				Expect(18);
				if (type == obj.type)
				if(obj.level == 0)
				  gen.StoreIndexedGlobal(reg,obj.adr,ix,name);
				else gen.StoreIndexedLocal(reg,tab.curLevel-obj.level, obj.adr,ix, name);
				
			} else if (la.kind == 27) {
				Get();
				if (obj.kind != var)
				  SemErr("cannot assign to procedure");
				
				Expr(out reg,
out type);
				if (type == obj.type)
				
				//check if the variable being referenced is a paramter
				if(obj.isParam==true){
				//get parameters offset
				 offset = obj.offset + 1;
				//get parameters value from the stack.
				 gen.storeParam(reg,offset,name);
				
				}
				else{
				  if (obj.level == 0)
				     gen.StoreGlobal(reg, obj.adr, name);
				  else gen.StoreLocal(reg, tab.curLevel-obj.level, obj.adr, name);
				  }
				
				if (la.kind == 28) {
					Get();
					int l1,l2; l2=0; l1=0; 
					if (type == boolean) {
					l2 = gen.NewLabel();
					l1 = gen.NewLabel();
					
					//if evaluates to false branch to the next Statment, dont evalue first expression
					gen.BranchFalse(l2);
					}
					else SemErr("boolean type expected");
					
					Expr(out reg1,
out type1);
					if (type1 == obj.type)
					  if (obj.level == 0)
					     gen.StoreGlobal(reg1, obj.adr, name);
					  else gen.StoreLocal(reg1, tab.curLevel-obj.level, obj.adr, name);
					  gen.Branch(l1);
					
					  //start label for the next expression
					  gen.Label(l2);
					
					Expect(29);
					Expr(out reg2,
out type2);
					if (type2 == obj.type)
					  if (obj.level == 0)
					     gen.StoreGlobal(reg2, obj.adr, name);
					  else gen.StoreLocal(reg2, tab.curLevel-obj.level, obj.adr, name);
					
					gen.Label(l1);
					
					
				}
				Expect(18);
			} else if (la.kind == 30) {
				Get();
				SemErr("cannot redefine a constant variable");
				        
			} else if (la.kind == 10) {
				Get();
				if (obj.kind != proc)
				{
				  SemErr("object is not a procedure");
				}
				cnt = obj.paramList.Count - 1;
				
				while (la.kind == 2) {
					Ident(out name2);
					param = tab.Find(name2);
					
					//type checking
					if(cnt >= 0 ){
					typeObj = obj.typeList[cnt];
					type = Convert.ToInt32(typeObj);
					
					if(param.type != type){
					 SemErr("parameter type mismatch in procedure " + name);
					}
					
					}
					
					
					//parameter count checking
					else{
					SemErr("number of parameters do not match in procedure " + name);
					}
					
					if (param.kind == var)
					{
					 adrReg = gen.GetRegister();
					
					 //increment number of parameter
					 numofParams = numofParams + 1;
					
					
					 //global or local variable?
					 if (param.level == 0)
					 {
					   SemErr(name2 + " is global, only local variables allowed to be passed ");
					
					 }
					 else
					 {
					    //if variable being passed is a local variable
					    gen.LoadLocalAddress(tab.curLevel - param.level, param.adr);
					    gen.MoveRegister(adrReg,2);
					
					 }
					
					 //push the parameter onto stack
					   gen.pushParam(adrReg);
					   cnt = cnt -1;
					}
					
					else SemErr("variable expected");
					
					
					Expect(18);
				}
				Expect(11);
				Expect(18);
				if(numofParams != obj.paramCount)
				{
				 SemErr("number of parameters do not match for function " + name);
				}
				gen.Call(name);
				
				
			} else SynErr(56);
			break;
		}
		case 31: {
			Get();
			int thisReg,NthisReg,esc; int nextCase = 0; esc = 0;
			Expect(10);
			Expr(out reg,out type);
			gen.ClearRegisters();
			thisReg = gen.GetRegister();
			
			Expect(11);
			Expect(19);
			Expect(32);
			Expr(out reg2,out type2);
			if(type2!=type){
			SemErr("type of switch case do not match");
			}
			else{
			
			 //comapre the case value with the switch variable
			
			 gen.RelOp(Op.EQU,reg2,thisReg);
			 nextCase = gen.NewLabel();
			 esc = gen.NewLabel();
			
			 //if not equal branch to next case, to check its value with it
			 gen.BranchFalse(nextCase);
			
			 gen.ClearRegisters();
			
			}
			
			Expect(29);
			if (StartOf(4)) {
				Stat();
				Expect(33);
				Expect(18);
				gen.Branch(esc);   
			}
			while (la.kind == 32) {
				Get();
				gen.Label(nextCase);
				gen.ClearRegisters();
				NthisReg = gen.GetRegister();
				gen.MoveRegister(NthisReg,thisReg);
				
				Expr(out reg1,out type1);
				if(type1 != type){
				SemErr("type of switch case do not match");
				}
				else{
				
				//compare case value with switch variable
				gen.RelOp(Op.EQU,reg1,thisReg);
				nextCase = gen.NewLabel();
				gen.BranchFalse(nextCase);
				}
				
				
				Expect(29);
				if (StartOf(4)) {
					Stat();
					Expect(33);
					Expect(18);
					gen.Branch(esc); 
				}
			}
			Expect(34);
			gen.Label(nextCase);  
			Expect(29);
			if (StartOf(4)) {
				Stat();
				Expect(33);
				Expect(18);
			}
			Expect(20);
			gen.Label(esc);
			
			break;
		}
		case 35: {
			Get();
			int l1, l2; l1 = 0; 
			Expr(out reg,
out type);
			if (type == boolean) {
			  l1 = gen.NewLabel();
			  gen.BranchFalse(l1);
			}
			else SemErr("boolean type expected");
			
			Stat();
			l2 = gen.NewLabel();
			gen.Branch(l2);
			gen.Label(l1);
			
			if (la.kind == 36) {
				Get();
				Stat();
			}
			gen.Label(l2); 
			break;
		}
		case 37: {
			Get();
			int l1, l2;
			l1 = gen.NewLabel();
			gen.Label(l1); l2=0;
			
			Expr(out reg,
out type);
			if (type == boolean) {
			  l2 = gen.NewLabel();
			  gen.BranchFalse(l2);
			}
			else SemErr("boolean type expected");
			
			Stat();
			gen.Branch(l1);
			gen.Label(l2);
			
			break;
		}
		case 38: {
			Get();
			Expect(10);
			Stat();
			int l1,l2; l2=0;first=0;
			first = gen.NewLabel();
			gen.Branch(first);
			
			//branch label to check condition again
			l1 = gen.NewLabel();
			gen.Label(l1);
			
			Stat();
			Expr(out reg,
out type);
			if (type == boolean) {
			  l2 = gen.NewLabel();
			
			  //if the expression is false, branch to end else re-execute
			  gen.BranchFalse(l2);
			}
			else SemErr("boolean type expected");
			
			Expect(18);
			Expect(11);
			Expect(39);
			Expect(19);
			gen.Label(first);  
			Stat();
			gen.Branch(l1);
			
			//end of the for loop, branch here once the terminating condition becomes false
			gen.Label(l2);
			
			Expect(20);
			break;
		}
		case 40: {
			Get();
			Ident(out name);
			Expect(18);
			obj = tab.Find(name);
			if (obj.type == integer) {
			  gen.ReadInteger();
			  if (obj.level == 0)
			     gen.StoreGlobal(0, obj.adr, name);
			  else gen.StoreLocal(0, tab.curLevel-obj.level, obj.adr, name);
			}
			else SemErr("integer type expected");
			
			break;
		}
		case 41: {
			Get();
			string text; 
			if (StartOf(5)) {
				Expr(out reg,
out type);
				switch (type) {
				  case integer: gen.WriteInteger(reg, false);
				                break;
				  case boolean: gen.WriteBoolean(false);
				                break;
				}
				
			} else if (la.kind == 3) {
				String(out text);
				gen.WriteString(text); 
			} else SynErr(57);
			Expect(18);
			break;
		}
		case 42: {
			Get();
			Expr(out reg,
out type);
			switch (type) {
			  case integer: gen.WriteInteger(reg, true);
			                break;
			  case boolean: gen.WriteBoolean(true);
			                break;
			}
			
			Expect(18);
			break;
		}
		case 19: {
			Get();
			tab.OpenSubScope(); 
			while (StartOf(3)) {
				if (la.kind == 44 || la.kind == 45) {
					VarDecl();
				} else if (la.kind == 48) {
					ArrayDecl();
				} else {
					ConstDef();
				}
			}
			Stat();
			while (StartOf(4)) {
				Stat();
			}
			Expect(20);
			tab.CloseSubScope(); 
			break;
		}
		default: SynErr(58); break;
		}
	}

	void Term(out int reg,        // load value of Term into register
out int type) {
		int typeR, regR; Op op; 
		Primary(out reg,
out type);
		while (StartOf(6)) {
			MulOp(out op);
			Primary(out regR,
out typeR);
			if (type == integer && typeR == integer)
			  gen.MulOp(op, reg, regR);
			else SemErr("integer type expected");
			
		}
	}

	void Tastier() {
		string progName; 
		Expect(43);
		Ident(out progName);
		tab.OpenScope(); 
		Expect(19);
		while (StartOf(3)) {
			if (la.kind == 48) {
				ArrayDecl();
			} else if (la.kind == 44 || la.kind == 45) {
				VarDecl();
			} else {
				ConstDef();
			}
		}
		while (StartOf(4)) {
			Stat();
		}
		while (la.kind == 17) {
			ProcDecl(progName);
		}
		tab.CloseScope(); 
		Expect(20);
	}



	public void Parse() {
		la = new Token();
		la.val = "";
		Get();
		Tastier();
		Expect(0);

	}

	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,T,x, T,T,x,x, x,x,x,T, T,T,T,T, T,x,T,T, x,T,T,T, T,T,T,x, T,T,x,T, x,x,x,T, x,T,T,x, T,T,T,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, T,T,x,T, T,x,x},
		{x,x,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,T, x,T,T,x, T,T,T,x, x,x,x,x, x,x,x},
		{x,T,T,x, x,T,x,x, T,T,T,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, T,T,T,T, T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
    public System.IO.TextWriter errorStream = Console.Error; // error messages go to this stream - was Console.Out DMA
    public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "number expected"; break;
			case 2: s = "ident expected"; break;
			case 3: s = "string expected"; break;
			case 4: s = "\"+\" expected"; break;
			case 5: s = "\"-\" expected"; break;
			case 6: s = "\"[\" expected"; break;
			case 7: s = "\"]\" expected"; break;
			case 8: s = "\"true\" expected"; break;
			case 9: s = "\"false\" expected"; break;
			case 10: s = "\"(\" expected"; break;
			case 11: s = "\")\" expected"; break;
			case 12: s = "\"*\" expected"; break;
			case 13: s = "\"div\" expected"; break;
			case 14: s = "\"DIV\" expected"; break;
			case 15: s = "\"mod\" expected"; break;
			case 16: s = "\"MOD\" expected"; break;
			case 17: s = "\"void\" expected"; break;
			case 18: s = "\";\" expected"; break;
			case 19: s = "\"{\" expected"; break;
			case 20: s = "\"}\" expected"; break;
			case 21: s = "\"=\" expected"; break;
			case 22: s = "\"<\" expected"; break;
			case 23: s = "\">\" expected"; break;
			case 24: s = "\"!=\" expected"; break;
			case 25: s = "\"<=\" expected"; break;
			case 26: s = "\">=\" expected"; break;
			case 27: s = "\":=\" expected"; break;
			case 28: s = "\"?\" expected"; break;
			case 29: s = "\":\" expected"; break;
			case 30: s = "\"<-\" expected"; break;
			case 31: s = "\"switch\" expected"; break;
			case 32: s = "\"case\" expected"; break;
			case 33: s = "\"break\" expected"; break;
			case 34: s = "\"default\" expected"; break;
			case 35: s = "\"if\" expected"; break;
			case 36: s = "\"else\" expected"; break;
			case 37: s = "\"while\" expected"; break;
			case 38: s = "\"for\" expected"; break;
			case 39: s = "\"do\" expected"; break;
			case 40: s = "\"read\" expected"; break;
			case 41: s = "\"write\" expected"; break;
			case 42: s = "\"writeln\" expected"; break;
			case 43: s = "\"program\" expected"; break;
			case 44: s = "\"int\" expected"; break;
			case 45: s = "\"bool\" expected"; break;
			case 46: s = "\",\" expected"; break;
			case 47: s = "\"constant\" expected"; break;
			case 48: s = "\"array\" expected"; break;
			case 49: s = "??? expected"; break;
			case 50: s = "invalid AddOp"; break;
			case 51: s = "invalid RelOp"; break;
			case 52: s = "invalid Primary"; break;
			case 53: s = "invalid Primary"; break;
			case 54: s = "invalid MulOp"; break;
			case 55: s = "invalid Type"; break;
			case 56: s = "invalid Stat"; break;
			case 57: s = "invalid Stat"; break;
			case 58: s = "invalid Stat"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}

	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}

	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}