    LDR     R5, =100
    LDR     R2, =12
    STR     R5, [R4, R2, LSL #2] ; constX
; Procedure Subtract
SubtractBody
    LDR     R2, =0
    LDR     R5, [R4, R2, LSL #2] ; i
    LDR     R6, =1
    SUB     R5, R5, R6
    LDR     R2, =0
    STR     R5, [R4, R2, LSL #2] ; i
    MOV     TOP, BP         ; reset top of stack
    LDR     BP, [TOP,#12]   ; and stack base pointers
    LDR     PC, [TOP]       ; return from Subtract
Subtract
    LDR     R0, =2          ; current lexic level
    LDR     R1, =0          ; number of local variables
    BL      enter             ; build new stack frame
    B       SubtractBody
;----------------------------------------------------------------------------------------------------------------------
; Procedure Add
AddBody
    LDR R1, = 2             ;load offset
    LDR R0,=0                                       
    ADD R0,R0,R1,LSL #2       ;4bytes for each param
    SUB R0,BP,R0              ;point to param to be loaded from stack
    LDR R2, [R0]              ;load param's adr
    LDR R5,[R2]                ;load tots value    
    LDR     R6, =1
    ADD     R5, R5, R6
    LDR R1, = 2             ;load offset
    LDR R0,=0                                       
    ADD R0,R0,R1,LSL #2       ;4bytes for each param
    SUB R0,BP,R0              ;point to param to be loaded from stack
    LDR R2, [R0]              ;load param's adr
    STR R5,[R2]             ;tots
    LDR R1, = 1             ;load offset
    LDR R0,=0                                       
    ADD R0,R0,R1,LSL #2       ;4bytes for each param
    SUB R0,BP,R0              ;point to param to be loaded from stack
    LDR R2, [R0]              ;load param's adr
    LDR R5,[R2]                ;load num value    
    LDR     R6, =3
    ADD     R5, R5, R6
    LDR R1, = 1             ;load offset
    LDR R0,=0                                       
    ADD R0,R0,R1,LSL #2       ;4bytes for each param
    SUB R0,BP,R0              ;point to param to be loaded from stack
    LDR R2, [R0]              ;load param's adr
    STR R5,[R2]             ;num
    MOV     TOP, BP         ; reset top of stack
    LDR     BP, [TOP,#12]   ; and stack base pointers
    LDR     PC, [TOP]       ; return from Add
Add
    LDR     R0, =2          ; current lexic level
    LDR     R1, =2          ; number of local variables
    BL      enter             ; build new stack frame
    B       AddBody
;----------------------------------------------------------------------------------------------------------------------
; name: tots
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: num
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; Procedure SumUp
SumUpBody
    LDR     R5, =1
    LDR     R2, =1
    ADD     R2, R4, R2, LSL #2
    LDR     R3,=2
    STR     R5, [R2, R3, LSL #2] ; value of arrX[]
    LDR     R2, =1
    ADD     R2, R4, R2, LSL #2
    LDR     R3,=2
    LDR     R5, [R2, R3, LSL #2] ; value of arrX[]
    LDR     R6, =2
    ADD     R5, R5, R6
    LDR     R2, =1
    ADD     R2, R4, R2, LSL #2
    LDR     R3,=2
    STR     R5, [R2, R3, LSL #2] ; value of arrX[]
    LDR     R2, =1
    ADD     R2, R4, R2, LSL #2
    LDR     R3,=2
    LDR     R5, [R2, R3, LSL #2] ; value of arrX[]
    ADD     R2, BP, #16
    LDR     R1, =12
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; arrResult
    LDR     R5, =3
    ADD     R2, BP, #16
    LDR     R1, =3
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; t1
    LDR     R5, =10
    ADD     R2, BP, #16
    LDR     R1, =4
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; t2
    ADD     R2, BP, #16
    LDR     R1, =4
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; t2
    ADD     R2, BP, #16
    LDR     R1, =3
    ADD     R2, R2, R1, LSL #2
    LDR     R6, [R2]        ; t1
    CMP     R5, R6
    MOVGT   R5, #1
    MOVLE   R5, #0
    MOVS    R5, R5          ; reset Z flag in CPSR
    BEQ     L1              ; jump on condition false
    ADD     R2, BP, #16
    LDR     R1, =4
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; t2
    LDR     R6, =4
    ADD     R5, R5, R6
    ADD     R2, BP, #16
    LDR     R1, =5
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; t3
    B       L2
L1
    ADD     R2, BP, #16
    LDR     R1, =4
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; t2
    LDR     R6, =2
    SUB     R5, R5, R6
    ADD     R2, BP, #16
    LDR     R1, =5
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; t3
L2
    LDR     R5, =0
    ADD     R2, BP, #16
    LDR     R1, =7
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; r
    LDR     R5, =0
    ADD     R2, BP, #16
    LDR     R1, =8
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; ix
    B       L3
L4
    ADD     R2, BP, #16
    LDR     R1, =8
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; ix
    LDR     R6, =1
    ADD     R5, R5, R6
    ADD     R2, BP, #16
    LDR     R1, =8
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; ix
    ADD     R2, BP, #16
    LDR     R1, =8
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; ix
    LDR     R6, =7
    CMP     R5, R6
    MOVLT   R5, #1
    MOVGE   R5, #0
    MOVS    R5, R5          ; reset Z flag in CPSR
    BEQ     L5              ; jump on condition false
L3
    ADD     R2, BP, #16
    LDR     R1, =7
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; r
    LDR     R6, =1
    ADD     R5, R5, R6
    ADD     R2, BP, #16
    LDR     R1, =7
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; r
    B       L4
L5
    LDR     R5, =7
    ADD     R2, BP, #16
    LDR     R1, =9
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; addNum
    LDR     R5, =0
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; total
    ADD     R2, BP, #16
    LDR     R1, =9
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; addNum
    LDR     R6, =2
    CMP     R6, R5
    MOVEQ   R6, #1
    MOVNE   R6, #0
    MOVS    R6, R6          ; reset Z flag in CPSR
    BEQ     L6              ; jump on condition false
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; total
    LDR     R6, =2
    ADD     R5, R5, R6
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; total
    B       L7
L6
    MOV     R5, R5
    LDR     R6, =1
    CMP     R6, R5
    MOVEQ   R6, #1
    MOVNE   R6, #0
    MOVS    R6, R6          ; reset Z flag in CPSR
    BEQ     L8              ; jump on condition false
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; total
    LDR     R6, =1
    ADD     R5, R5, R6
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; total
    B       L7
L8
    MOV     R5, R5
    LDR     R6, =7
    CMP     R6, R5
    MOVEQ   R6, #1
    MOVNE   R6, #0
    MOVS    R6, R6          ; reset Z flag in CPSR
    BEQ     L9              ; jump on condition false
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; total
    LDR     R6, =7
    ADD     R5, R5, R6
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; total
    B       L7
L9
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; total
    LDR     R6, =0
    ADD     R5, R5, R6
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; total
L7
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    MOV     R5, R2
    STR     R5, [TOP]           ;store parameter  
    ADD     TOP, TOP,#4           ;update TOP
    ADD     R2, BP, #16
    LDR     R1, =9
    ADD     R2, R2, R1, LSL #2
    MOV     R6, R2
    STR     R6, [TOP]           ;store parameter  
    ADD     TOP, TOP,#4           ;update TOP
    ADD     R0, PC, #4      ; store return address
    STR     R0, [TOP]       ; in new stack frame
    B       Add
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    LDR     R7, [R2]        ; total
    LDR     R8, =2
    ADD     R7, R7, R8
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    STR     R7, [R2]        ; total
    ADD     R0, PC, #4      ; string address
    BL      TastierPrintString
    B       L10
    DCB     "total: ", 0
    ALIGN
L10
    ADD     R2, BP, #16
    LDR     R1, =11
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; total
    MOV     R0, R5
    BL      TastierPrintIntLf
    ADD     R0, PC, #4      ; string address
    BL      TastierPrintString
    B       L11
    DCB     "result of arrX[2]: ", 0
    ALIGN
L11
    ADD     R2, BP, #16
    LDR     R1, =12
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; arrResult
    MOV     R0, R5
    BL      TastierPrintIntLf
    ADD     R0, PC, #4      ; string address
    BL      TastierPrintString
    B       L12
    DCB     "t3: ", 0
    ALIGN
L12
    ADD     R2, BP, #16
    LDR     R1, =5
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; t3
    MOV     R0, R5
    BL      TastierPrintIntLf
    ADD     R0, PC, #4      ; string address
    BL      TastierPrintString
    B       L13
    DCB     "result of r after for loop: ", 0
    ALIGN
L13
    ADD     R2, BP, #16
    LDR     R1, =7
    ADD     R2, R2, R1, LSL #2
    LDR     R5, [R2]        ; r
    MOV     R0, R5
    BL      TastierPrintIntLf
    MOV     TOP, BP         ; reset top of stack
    LDR     BP, [TOP,#12]   ; and stack base pointers
    LDR     PC, [TOP]       ; return from SumUp
SumUp
    LDR     R0, =1          ; current lexic level
    LDR     R1, =15          ; number of local variables
    BL      enter             ; build new stack frame
    B       SumUpBody
;----------------------------------------------------------------------------------------------------------------------
; name: j
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: sum
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: q
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: t1
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: t2
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: t3
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: t4
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: r
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: ix
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: addNum
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: temp
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: total
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: arrResult
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: constRes
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: e
;kind : local var
;type : booleaan

;----------------------------------------------------------------------------------------------------------------------
; name: Subtract
;kind : procedure
;type : undefined

;----------------------------------------------------------------------------------------------------------------------
; name: Add
;kind : procedure
;type : undefined

;----------------------------------------------------------------------------------------------------------------------
    LDR     R5, =100
    ADD     R2, BP, #16
    LDR     R1, =0
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; constX
MainBody
    LDR     R5, =100
    ADD     R2, BP, #16
    LDR     R1, =0
    ADD     R2, R2, R1, LSL #2
    LDR     R6, [R2]        ; constX
    ADD     R5, R5, R6
    ADD     R2, BP, #16
    LDR     R1, =1
    ADD     R2, R2, R1, LSL #2
    STR     R5, [R2]        ; x
    ADD     R0, PC, #4      ; store return address
    STR     R0, [TOP]       ; in new stack frame
    B       SumUp
StopTest
    B       StopTest
Main
    LDR     R0, =1          ; current lexic level
    LDR     R1, =2          ; number of local variables
    BL      enter             ; build new stack frame
    B       MainBody
;----------------------------------------------------------------------------------------------------------------------
; name: constX
;kind : constant
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: x
;kind : local var
;type : int

;----------------------------------------------------------------------------------------------------------------------
;----------------------------------------------------------------------------------------------------------------------
; name: i
;kind : global var
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: arrX
;kind : array
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: constX
;kind : constant
;type : int

;----------------------------------------------------------------------------------------------------------------------
; name: SumUp
;kind : procedure
;type : undefined

;----------------------------------------------------------------------------------------------------------------------
; name: main
;kind : procedure
;type : undefined

;----------------------------------------------------------------------------------------------------------------------
