using System;
using System.Collections;
namespace Tastier {

public class Obj { // properties of declared symbol
   public string name; // its name
   public int kind;    // var, proc, scope or array
   public int type;    // its type if var (undef for proc)
   public int level;   // lexic level: 0 = global; >= 1 local
   public int adr;     // address (displacement) in scope
   public Obj next;    // ptr to next object in scope
   // for scopes
   public Obj outer;   // ptr to enclosing scope
   public Obj locals;  // ptr to locally declared objects
   public int nextAdr; // next free address in scope
   public bool isConst;
   public bool isScalar;
   public bool isParam;
   public int dimension;
   public int length;
   public int offset;
   public int paramCount;
   public ArrayList paramList;
   public ArrayList typeList;
}

public class SymbolTable {

   const int // object kinds
      var = 0, proc = 1, scope = 2, constant =3, array = 4;

   const int // types
      undef = 0, integer = 1, boolean = 2;

   public Obj topScope; // topmost procedure scope
   public int curLevel; // nesting level of current scope
   public Obj undefObj; // object node for erroneous symbols

   public bool mainPresent;

   Parser parser;

   public SymbolTable(Parser parser) {
      curLevel = -1;
      topScope = null;
      undefObj = new Obj();
      undefObj.name = "undef";
      undefObj.kind = var;
      undefObj.type = undef;
      undefObj.level = 0;
      undefObj.adr = 0;
      undefObj.next = null;
      this.parser = parser;
      mainPresent = false;
   }

// open new scope and make it the current scope (topScope)
   public void OpenScope() {
      Obj scop = new Obj();
      scop.name = "";
      scop.kind = scope;
      scop.outer = topScope;
      scop.locals = null;
      scop.nextAdr = 0;
      topScope = scop;
      curLevel++;
   }

// close current scope
   public void CloseScope() {


            if (topScope != null)
            {

                Obj temp = topScope.locals;

                while (temp!= null)
                {

                    Console.WriteLine(";----------------------------------------------------------------------------------------------------------------------");

                    Console.WriteLine("; name: {0}", temp.name);



                  if (temp.kind == 0)
                    {
                  if(temp.level ==0){

                     Console.WriteLine(";kind : global var");

                  }
                  else{

                  Console.WriteLine(";kind : local var");


                  }
                    }
                    else if (temp.kind == 1)
                    {
                        Console.WriteLine(";kind : procedure");

                    }

                    else if (temp.kind == 2)
                    {
                        Console.WriteLine(";kind : scope");
                    }

                    else if (temp.kind == 3)
                    {
                        Console.WriteLine(";kind : constant");
                    }


                    else if (temp.kind == 4)
                    {
                        Console.WriteLine(";kind : array");
                    }


                    else
                    {
                        Console.WriteLine(";kind : not known");

                    }



               if (temp.type == 0)
                    {
                        Console.WriteLine(";type : undefined");

                    }
                    else if (temp.type == 1)
                    {
                        Console.WriteLine(";type : int");

                    }

                    else if (temp.type == 2)
                    {
                        Console.WriteLine(";type : booleaan");

                    }
                    else
                    {
                        Console.WriteLine(";type : none");

                    }




                    Console.WriteLine("");

                    temp = temp.next;


                }



            }

         Console.WriteLine(";----------------------------------------------------------------------------------------------------------------------");

            topScope = topScope.outer;

            curLevel--;
   }  //close scope

// open new sub-scope and make it the current scope (topScope)
   public void OpenSubScope() {
   // lexic level remains unchanged
      Obj scop = new Obj();
      scop.name = "";
      scop.kind = scope;
      scop.outer = topScope;
      scop.locals = null;
   // next available address in stack frame remains unchanged
      scop.nextAdr = topScope.nextAdr;
      topScope = scop;
   }

// close current sub-scope
   public void CloseSubScope() {
   // update next available address in enclosing scope
      topScope.outer.nextAdr = topScope.nextAdr;
   // lexic level remains unchanged
      topScope = topScope.outer;
   }


//allocate space for array

  public void allocateArray(string name,int length){
    Obj array = Find(name);

    array.adr = topScope.nextAdr++;


      topScope.nextAdr += length ;

      array.length = length;

  }



// create new object node in current scope
   public Obj NewObj(string name, int kind, int type) {
      Obj p, last;
      Obj obj = new Obj();
      obj.name = name; obj.kind = kind;
      obj.type = type; obj.level = curLevel;
      obj.next = null;
      obj.isParam = false;

      if(obj.kind == array){
        obj.isScalar = false;
        obj.isConst = false;
      }
      else{
        obj.isScalar = true;
        if(obj.type == constant){
          obj.isConst = true;
        }
        else{
          obj.isConst = false;
        }
      }

      p = topScope.locals; last = null;

      while (p != null) {
         if (p.name == name)
            parser.SemErr("name declared twice");
         last = p; p = p.next;
      }

      if (last == null)
         topScope.locals = obj; else last.next = obj;

      if(obj.isScalar == true){
         if (kind == var | kind == constant)
            obj.adr = topScope.nextAdr++;
      }

      else {

         if(obj.kind == array){

            obj.length =0;




         }
      }

      if(obj.kind == proc){
        obj.paramCount =0;
        obj.paramList = new ArrayList();
        obj.typeList = new ArrayList();
      }



      return obj;


   }



// search for name in open scopes and return its object node
   public Obj Find(string name) {
      Obj obj, scope;
      scope = topScope;
      while (scope != null) { // for all open scopes
         obj = scope.locals;
         while (obj != null) { // for all objects in this scope
            if (obj.name == name) return obj;
            obj = obj.next;
         }
         scope = scope.outer;
      }
      parser.SemErr(name + " is undeclared");
      return undefObj;
   }

} // end SymbolTable

} // end namespace
