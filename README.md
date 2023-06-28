# Auto refactor your legacy VB.NET project respecting the naming conventions

## Overview

So you have this manager that asks you to run a legacy VB.NET project against [SonarQube](https://www.sonarsource.com/) and correct all naming warnings that might be highlighted. It happens that the project isn't small and SonarQube doesn't care much about your patience level and you end up with thousands of methods and their parameters and even their local variables to refactor.
This is the moment you start a painful, repetitive process... or you **code**!

This project is the result of a non-patient programmer building a .NET 6 Console Application to do this job for him. Of course, that GitHub had to be used to make sure that nothing is refactored unduly.

## **VBCodeCompliancer** Structure

For each file that is read and refactored, the **VBCodeCompliancer** will create an object called **HandlingFile** will its content as a list of strings. Once the **VBCodeCompliancer** was built upon a _Chain Of Responsibility_ design pattern, you will see a list of _Handlers_ that will be called one by one where each one searches and refactors one specific item of the code in the current file (one _handler_ for methods names, other for parameters, other for local variables...).

The main class is the **CoRClient** (where the _handlers_ are initiated) and their order is established.

The handlers themselves inherit from the **AbstractHandler** which implements the **IHandler** interface. If you want to add a new _handler_ just make sure it inherits from this abstract class and add it to the chain.

## Running the App

After running the _.exe_ (or with Visual Studio) you will be prompted to introduce the project path you want to refactor. There must be one (and only one) _.sln_ file in the folder you specified.

Secondly, the app will use the _.sln_ file to search for all the projects within the solution.

Then, all _.vb_ files visible to the **VBCodeCompliancer** are printed to the console. You can decide to refactor all at once or confirm one by one before it is refactored.

Lastly, the app will search and refactor all methods, parameters and variables names (at this first version) that doesn't obey to the naming convention listed below (these are the naming convention required by [SonarQube](https://www.sonarsource.com/)):

1. Method names:
```C#
^([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$
```
2. Parameter names:
```C#
^[a-z][a-z0-9]*([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$
```
3. Local Variables names:
```C#
^[a-z][a-z0-9]*([A-Z]{1,3}[a-z0-9]+)*([A-Z]{2})?$
```

## Example

On this same folder, you have a **VBCodeCompliancerExample** solution which you can try refactoring. You just run the **VBCodeCompliancer** and give it the **VBCodeCompliancerExample** path and you will see that the example project still compiles and runs!


### Module1 class in the main project:

```VB
Dim _v1, _v2 As Integer
```
to
```VB
Dim v1, v2 As Integer
```
without changing the strings
```VB
"_v1: " | "_v2: "
```

### _TestClass_ in the _ClassLibrary_ project:
```VB
Public Function _Sum_2_Values(ByVal value1 As Integer, ByVal value2 As Integer)
    Dim result1231_to_return As Integer

    Su_m(value1, value2, result1231_to_return)

    Return result1231_to_return
End Function

Private Sub Su_m(ByVal _v1 As Integer, ByVal _v_2 As Integer, ByRef _result As Integer)
    _result = _v1 + _v_2
End Sub
```
to
```VB
Public Function Sum2Values(ByVal value1 As Integer, ByVal value2 As Integer)
    Dim result1231ToReturn As Integer

    Sum(value1, value2, result1231ToReturn)

    Return result1231ToReturn
End Function

Private Sub Sum(ByVal v1 As Integer, ByVal v2 As Integer, ByRef result As Integer)
    result = v1 + v2
End Sub
```


## Want to Contribute?

This project doesn't include a handler for the properties of a class :)

## What's Next?

Probably I will be asked to do the same kind of job in a C#.NET project, so there will be a CsharpCodeCompliancer soon!