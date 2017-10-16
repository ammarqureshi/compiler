## A Minimalistic compiler ##


The project implements a basic compiler by applying syntax-directed translation. ARM assembly language is used as the target. The compiler is implemented using Coco/R which is a compiler generator and generates a recursive descent parser and scanner given an ATG(Attributed Translation Grammer), symbol table and a code generator.


Implementing functionalities such as:

1. scalar and non scalar variables
2. conditional statements: if statements, switch cases and ternary operators
3. parameter passing by value
4. compile-time array bound checking


To compile the project on a Unix machine:
1. run 'brew install make'
2. run 'make build'
3. run 'make compile'


Here is a detailed report of my implementation:
(https://github.com/ammarqureshi/compiler/files/1385328/Combined.Report.pdf)
