using System;
using System.IO;

namespace Tastier {

public enum Op { // opcodes
   ADD, SUB, MUL, DIV, MOD,
   EQU, LSS, LEQ, GTR, GEQ, NEQ
}

public class CodeGenerator {

   int nextLabel;
   int nextFreeRegister;

   public CodeGenerator() {
      nextLabel = 1;
      nextFreeRegister = 5;
   }

   //----- numeric label generator -----

   public int NewLabel() {
      return nextLabel++;
   }

   //----- register allocator methods -----

   public int GetRegister() {
      return nextFreeRegister++;
   }

   public void ClearRegisters() {
      nextFreeRegister = 5;
   }

//----- code generation methods -----

// stack frame layout and pointer locations:
//
//       *                         *
// top ->*                         *
//       * local variables         *
//       ***************************
//       * dynamic link (dl)       *
//       * static link (sl)        *
//       * lexic level delta (lld) *
// bp -> * return address          *
//       ***************************
//
// dl - bp of calling procedure's frame for popping stack
// sl - bp of enclosing procedure for addressing nonlocal variables
// ll - procedure's lexic level
//
// note: procedure's name is declared at ll-1

// method to generate ARM assembly language label
   public void Label(string name, string subName) {
      Console.WriteLine("{0}{1}", name, subName);
   }

// method to generate ARM assembly language numeric label
   public void Label(int number) {
      Console.WriteLine("L{0}", number);
   }

// method to generate ARM assembly language code to stop program
   public void StopProgram(string name) {
      Console.WriteLine("Stop{0}", name);
      Console.WriteLine("    B       Stop{0}", name);
   }

// method to generate ARM assembly language code to reset Z flag
   public void ResetZ(int register) {
      Console.WriteLine("    MOVS    R{0}, R{0}          ; reset Z flag in CPSR", register);
   }

// method to generate ARM assembly language code for branch instruction
   public void Branch(int label) {
      Console.WriteLine("    B       L{0}",label);
   }

// method to generate ARM assembly language code for branch on codition true instruction
   public void BranchTrue(int label) {
      Console.WriteLine("    BNE     L{0}              ; jump on condition true", label);
   }

// method to generate ARM assembly language code for branch on codition false instruction
   public void BranchFalse(int label) {
      Console.WriteLine("    BEQ     L{0}              ; jump on condition false", label);
   }

// method to generate ARM assembly language code to load constant value
   public void LoadConstant(int register, int value) {
      Console.WriteLine("    LDR     R{0}, ={1}", register, value);
   }

// method to generate ARM assembly language code to load constant value true
   public void LoadTrue(int register) {
   // load value and clear Z flag in CPSR
      Console.WriteLine("    MOVS    R{0}, #1          ; true", register);
   }

// method to generate ARM assembly language code to load constant value false
   public void LoadFalse(int register) {
   // load value and set Z flag in CPSR
      Console.WriteLine("    MOVS    R{0}, #0          ; false", register);
   }

// method to generate ARM assembly language code to negate value in register
   public void NegateValue(int register) {
      Console.WriteLine("    RSB     R{0}, R{0}, #0", register);
   }

// method to generate ARM assembly language code to move value from register to register
   public void MoveRegister(int r1, int r2) {
      Console.WriteLine("    MOV     R{0}, R{1}", r1, r2);
   }
// method to generate ARM assembly laguage code to load local variable's address
   public void LoadLocalAddress(int llDelta, int offset) {
      if (llDelta == 0) { // variable is in current stack frame
         Console.WriteLine("    ADD     R2, BP, #16");
         Console.WriteLine("    LDR     R1, ={0}", offset);
         // scalar variables are four bytes long
         Console.WriteLine("    ADD     R2, R2, R1, LSL #2");
      } else { // move llDelta steps down static link chain
         Console.WriteLine("    MOV     R2, BP          ; load current base pointer");
         while (llDelta > 0) {
            llDelta--;
            Console.WriteLine("    LDR     R2, [R2,#8]");
         }
         if (offset == 0)
            Console.WriteLine("    ADD     R2, R2,#16");
         else { // compute variable's address in stack frame
            Console.WriteLine("    ADD     R2, R2, #16");
            Console.WriteLine("    LDR     R1, ={0}", offset);
            // scalar variables are four bytes long
            Console.WriteLine("    ADD     R2, R2, R1, LSL #2");
         }
      }
   }

// method to generate ARM assembly laguage code to load local variable's value
   public void LoadLocalValue(int register, string name) {
      Console.WriteLine("    LDR     R{0}, [R2]        ; {1}", register, name);
   }

// method to generate ARM assembly laguage code to load local variable
   public void LoadLocal(int register, int llDelta, int offset, string name) {
      LoadLocalAddress(llDelta, offset);
      LoadLocalValue(register, name);
   }

// method to generate ARM assembly laguage code to store local variable's value
   public void StoreLocalValue(int register, string name) {
      Console.WriteLine("    STR     R{0}, [R2]        ; {1}", register, name);
   }

// method to generate ARM assembly laguage code to store local variable
   public void StoreLocal(int register, int llDelta, int offset, string name) {
      LoadLocalAddress(llDelta, offset);
      StoreLocalValue(register, name);
   }

// method to generate ARM assembly laguage code to load local indexed variable's address
   public void LoadIndexedLocalAddress(int llDelta, int offset) {
      if (llDelta == 0) // variable is in current stack frame
         Console.WriteLine("    ADD     R2, BP, #16");
      else { // move llDelta steps down static link chain
         Console.WriteLine("    MOV     R2, BP          ; load current base pointer");
            while (llDelta > 0) {
               llDelta--;
               Console.WriteLine("    LDR     R2, [R2,#8]");
            }
            Console.WriteLine("    ADD     R2, R2, #16");
         }
      if (offset > 0) {
         Console.WriteLine("    LDR     R1, ={0}", offset);
         Console.WriteLine("    ADD     R2, R2, R1, LSL #2");
      }
   }

// method to generate ARM assembly laguage code to load local indexed variable's value
   public void LoadIndexedLocalValue(int register, int index, string name) {
      // array elements are four bytes long
      Console.WriteLine("    LDR     R{0}, [R2, R{1}, LSL #2] ; value of {2}[]", register, index, name);
   }

// method to generate ARM assembly laguage code to load local indexed variable
   public void LoadIndexedLocal(int register, int llDelta, int offset, int index, string name) {
      LoadIndexedLocalAddress(llDelta, offset);
      LoadIndexedLocalValue(register, index, name);
   }
// method to generate ARM assembly laguage code to store local indexed variable's value
      public void StoreIndexedLocalValue(int register, int index, string name) {
      // array elements are four bytes long
      Console.WriteLine("    STR     R{0}, [R2, R{1}, LSL #2] ; value of {2}[]", register, index, name);
   }

// method to generate ARM assembly laguage code to store local indexed variable
   public void StoreIndexedLocal(int register, int llDelta, int offset, int index, string name) {
      LoadIndexedLocalAddress(llDelta, offset);
      StoreIndexedLocalValue(register, index, name);
   }
// method to generate ARM assembly laguage code to load global variable's address
   public void LoadGlobalAddress(int address) {
      Console.WriteLine("    LDR     R2, ={0}", address);
   }

// method to generate ARM assembly laguage code to load global variable's value
   public void LoadGlobalValue(int register, string name) {
   // R4 holds base address for globals, scalar variables are four bytes long
      Console.WriteLine("    LDR     R{0}, [R4, R2, LSL #2] ; {1}", register, name);
   }

// method to generate ARM assembly laguage code to load global variable
   public void LoadGlobal(int register, int address, string name) {
      LoadGlobalAddress(address);
      LoadGlobalValue(register, name);
   }
// method to generate ARM assembly laguage code to store global variable's value
   public void StoreGlobalValue(int register, string name) {
   // R4 holds base address for globals, scalar variables are four bytes long
      Console.WriteLine("    STR     R{0}, [R4, R2, LSL #2] ; {1}", register, name);
   }

// method to generate ARM assembly laguage code to store global variable
   public void StoreGlobal(int register, int address, string name) {
      LoadGlobalAddress(address);
      StoreGlobalValue(register, name);
   }
// method to generate ARM assembly laguage code to load indexed global variable's address
      public void LoadIndexedGlobalAddress(int address) {
      Console.WriteLine("    LDR     R2, ={0}", address);
      Console.WriteLine("    ADD     R2, R4, R2, LSL #2");
   }

// method to generate ARM assembly laguage code to load global indexed variable's value
   public void LoadIndexedGlobalValue(int register, int index, string name) {
   // R4 holds base address for globals, scalar variables and array elements are four bytes long
      Console.WriteLine("    LDR     R3,={0}", index);
      Console.WriteLine("    LDR     R{0}, [R2, R3, LSL #2] ; value of {1}[]", register, name);
   }

// method to generate ARM assembly laguage code to load global indexed variable
   public void LoadIndexedGlobal(int register, int address, int index, string name) {
      LoadIndexedGlobalAddress(address);
      LoadIndexedGlobalValue(register, index, name);
   }
// method to generate ARM assembly laguage code to store indexed global variable' value
   public void StoreIndexedGlobalValue(int register, int index, string name) {
   // R4 holds base address for globals, scalar variables and array elements are four bytes long
      Console.WriteLine("    LDR     R3,={0}", index);
      Console.WriteLine("    STR     R{0}, [R2, R3, LSL #2] ; value of {1}[]", register, name);
   }

// method to generate ARM assembly laguage code to store indexed global variable
   public void StoreIndexedGlobal(int register, int address, int index, string name) {
      LoadIndexedGlobalAddress(address);
      StoreIndexedGlobalValue(register, index, name);
   }
// method to generate ARM assembly language code for relop
   public void RelOp(Op op, int leftOp, int rightOp) {
      Console.WriteLine("    CMP     R{0}, R{1}", leftOp, rightOp);
      switch (op) {
      // set true (value 1) if condition is met
         case Op.EQU: Console.WriteLine("    MOVEQ   R{0}, #1", leftOp);
                      Console.WriteLine("    MOVNE   R{0}, #0", leftOp);
                      break;
         case Op.LSS: Console.WriteLine("    MOVLT   R{0}, #1", leftOp);
                      Console.WriteLine("    MOVGE   R{0}, #0", leftOp);
                      break;
         case Op.LEQ: Console.WriteLine("    MOVLE   R{0}, #1", leftOp);
                      Console.WriteLine("    MOVGT   R{0}, #0", leftOp);
                      break;
         case Op.GTR: Console.WriteLine("    MOVGT   R{0}, #1", leftOp);
                      Console.WriteLine("    MOVLE   R{0}, #0", leftOp);
                      break;
         case Op.GEQ: Console.WriteLine("    MOVGE   R{0}, #1", leftOp);
                      Console.WriteLine("    MOVLT   R{0}, #0", leftOp);
                      break;
         case Op.NEQ: Console.WriteLine("    MOVNE   R{0}, #1", leftOp);
                      Console.WriteLine("    MOVEQ   R{0}, #0", leftOp);
                      break;
      }
      Console.WriteLine("    MOVS    R{0}, R{0}          ; reset Z flag in CPSR", leftOp);
   }

// method to generate ARM assembly language code for addop
   public void AddOp(Op op, int leftOp, int rightOp) {
      if (op == Op.ADD)
         Console.WriteLine("    ADD     R{0}, R{0}, R{1}", leftOp, rightOp);
      else
         Console.WriteLine("    SUB     R{0}, R{0}, R{1}", leftOp, rightOp);
   }

// method to generate ARM assembly language code for
   public void MulOp(Op op, int leftOp, int rightOp) {
      switch (op) {
         case Op.MUL: // note: ordering of operands in generated instruction
                      Console.WriteLine("    MUL     R{0}, R{1}, R{0}", leftOp, rightOp);
                      break;
         case Op.DIV: Console.WriteLine("    MOV     R0, R{0}          ; load left operand", leftOp);
                      Console.WriteLine("    MOV     R1, R{0}          ; load right operand", rightOp);
                      Console.WriteLine("    BL      TastierDiv");
                      Console.WriteLine("    MOV     R{0}, R0          ; recover result", leftOp);
                      break;
         case Op.MOD: Console.WriteLine("    MOV     R0, R{0}          ; load left operand", leftOp);
                      Console.WriteLine("    MOV     R1, R{0}          ; load right operand", rightOp);
                      Console.WriteLine("    BL      TastierMod");
                      Console.WriteLine("    MOV     R{0}, R0          ; recover result", leftOp);
                      break;
      }
   }

// method to generate ARM assembly language code to print integer vaue
   public void WriteInteger(int register, bool newLine) {
      Console.WriteLine("    MOV     R0, R{0}", register);
      if (newLine)
         Console.WriteLine("    BL      TastierPrintIntLf");
      else
         Console.WriteLine("    BL      TastierPrintInt");
   }

// method to generate ARM assembly language code to print boolean vaue
   public void WriteBoolean(bool newLine) {
      if (newLine) {
         Console.WriteLine("    BLNE    TastierPrintTrueLf");
         Console.WriteLine("    BLEQ    TastierPrintFalseLf");
         }
      else {
         Console.WriteLine("    BLNE    TastierPrintTrue");
         Console.WriteLine("    BLEQ    TastierPrintFalse");
      }
   }

// method to generate ARM assembly language code to print string vaue
   public void WriteString(string text) {
      int l;
      l = NewLabel();
      Console.WriteLine("    ADD     R0, PC, #4      ; string address");
      Console.WriteLine("    BL      TastierPrintString");
      Branch(l);
      Console.WriteLine("    DCB     {0}, 0", text);
      Console.WriteLine("    ALIGN");
      Label(l);
   }

// method to generate ARM assembly language code to read integer vaue
   public void ReadInteger() {
      // note: integer value returned in R0
      Console.WriteLine("    BL      TastierReadInt");
   }

// method to generate ARM assembly language procdure name comment
   public void ProcNameComment(string name) {
      Console.WriteLine("; Procedure {0}", name);
   }



//method to push parameter on to the stack before giving control to the callee procedure.
   public void pushParam(int reg){
     Console.WriteLine("    STR     R{0}, [TOP]           ;store parameter  " ,reg);
     Console.WriteLine("    ADD     TOP, TOP,#4           ;update TOP");
   }




// method to generate ARM assembly language code to call a procedure
  public void Call(string name) {
     Console.WriteLine("    ADD     R0, PC, #4      ; store return address");
     Console.WriteLine("    STR     R0, [TOP]       ; in new stack frame");
     Console.WriteLine("    B       {0}", name);
  }



   public void loadParam(int reg, int offset,string name){
     loadParamAdr(offset);
     loadParamValue(reg,name);
   }

   public void loadParamAdr(int offset){
    Console.WriteLine("    LDR R1, = {0}             ;load offset", offset );
    Console.WriteLine("    LDR R0,=0                                       ");
    Console.WriteLine("    ADD R0,R0,R1,LSL #2       ;4bytes for each param" );

    Console.WriteLine("    SUB R0,BP,R0              ;point to param to be loaded from stack");
    Console.WriteLine("    LDR R2, [R0]              ;load param's adr" );

   }

   public void loadParamValue(int reg,string name){
     Console.WriteLine("    LDR R{0},[R2]                ;load {1} value    ",reg,name);
   }

   public void storeParam(int reg,int offset,string name){
    loadParamAdr(offset);                               //the computed adr. of the param is in R2.
    Console.WriteLine("    STR R{0},[R2]             ;{1}",reg,name);
   }



// method to generate ARM assembly language code to enter a procedure
   public void Enter(string name, int level, int varCount) {
      Console.WriteLine("{0}", name);
      Console.WriteLine("    LDR     R0, ={0}          ; current lexic level", level);
      Console.WriteLine("    LDR     R1, ={0}          ; number of local variables", varCount);
      Console.WriteLine("    BL      enter             ; build new stack frame");
      Console.WriteLine("    B       {0}Body", name);
   }



// method to generate ARM assembly language code to return from a procedure
   public void Return(string name) {
      Console.WriteLine("    MOV     TOP, BP         ; reset top of stack");
      Console.WriteLine("    LDR     BP, [TOP,#12]   ; and stack base pointers");
      Console.WriteLine("    LDR     PC, [TOP]       ; return from {0}", name);
   }




} // end CodeGenerator

} // end namespace
