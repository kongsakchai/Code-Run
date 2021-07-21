# Unity-CodeRun
An interpreter for Basic Coding Game in Unity Engine. It's an interpreter designed based on the ["reading through Writing An Interpreter In Go"](https://interpreterbook.com/) book.

Features
- Variable (non-scorpe)
- double,boolean,string
- arithmetic
- if
- while
- call function

## Example
variabl
```
a=100; //integers(double)
b=100.1; //double
c=true; //boolean
d="Hello"; //string
```
arithmetic
```
a=10+14; // 24
b=2*5/10; // 1
c=true && false || b==1; //true
d="Hello " + b; // "Hello 1" 
```
if
```
if(a<5){
    a=0;
}else if(a<10){
    a=10;
}else{
    a=20;
}
```
while
```
a=0;
while(a<10){
    a=a+1;
}
```
call functin
```
a=10;
print(a); // 10
```
## Resources
- [Monkey-CSharp](https://github.com/ronnieholm/Monkey-CSharp) by [Ronnie Holm](https://github.com/ronnieholm)
