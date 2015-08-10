## Introduction ##

This article contains an overview of breaking changes in the upcoming Ela 0.11.

Ela 0.11 will be the last release with major breaking changes. Ela 0.11 is a finalization of a language concept, the next releases will be oriented towards stabilization, new functionality and enhancing of standard library and Ela tool chain.

## Syntax changes ##

### Global bindings ###

Now `let` keyword is not needed for global bindings. Ela syntax is now more Haskell like.

Global binding (Ela 0.10):

```
let map f (x::xs) = f x :: map xs
    map _ [] = []
```

Global binding (Ela 0.11):

```
map f (x::xs) = f x :: map xs
map _ [] = []
```

### Chain of local bindings ###

In Ela 0.10 it was possible to declare several bindings at once using
`let/in` and `where` constructs. In such cases bindings were separated
by `et` keyword. In Ela 0.11 this keyword is no longer needed.

Chaining bindings (Ela 0.10):

```
let z = x + y
        where x = 2
           et y = 3
```

Chaining bindings (Ela 0.11):

```
z = x + y
    where x = 2
          y = 3
```

### Binding attributes ###

Ela 0.10 supported binding attributes. Attribute were in fact keywords that should follow `let` (or `where`) keyword. Ela 0.11 also provides a support for attributes but through binding header. Attributes are no longer keywords.

Attributes (Ela 0.10):

```
let private sum x y = x + y
```

Attributes (Ela 0.11):

```
sum # private
sum x y = x + y
```

Binding header consists of a binding name, followed by `#` sign, followed by one or more attributes. Headers can be specified for both global and local bindings.

## Referencing modules ##

In Ela 0.10 only a single `open` directive was used for referencing modules. By default it was importing all names from a target module into a global scope. One could override this behavior using `qualified` attribute. In such case all names from a target module require full qualification (they should be prefixed with module alias).
Ela 0.11 introduces two directives instead - `open` directive and `import` directive.

Opening module (Ela 0.10):

```
open moduleName
```

Opening module (Ela 0.11):

```
open moduleName
```

Importing module, names should be qualified (Ela 0.10):

```
open qualified moduleName
```


Importing module, names should be qualified (Ela 0.11):

```
import moduleName
```


## Pattern matching ##

Syntax for type check pattern has changed.

Ela 0.10:

```
match x with
      ?int = ...
```

Ela 0.11:

```
match x with
      _ is int = ...
```

Ela 0.10:

```
match x with
      (?int)@name = ...
```

Ela 0.11:

```
match x with
      name is int = ...
```

Also type name can now be prefixed with the name of a module where this type is defined.

## Name shadowing ##

A name shadowing feature (an ability for bindings to hide each other even if they share the same scope) introduced in Ela 0.10 is now decomissioned. For example, the following code was valid in Ela 0.10:

```
let x = 2
let x = 4
```

In Ela 0.11 however a similar code is not valid and would result in a compilation error:

```
x = 2
x = 4
```

However shading in a function argument list is still supported and lambda definition like so: `\x x -> x` is still valid. The following code is valid as well:

```
fun x x = x
```

## Operator is ##

Ela 0.10 (and previous versions) supported a short-cut version of a `match` expression - an `is` expression:

```
(x is (x::xs)) = lst
```

It was useful in many cases however it had non-obvious scoping for the names declared in pattern. In Ela 0.11 this operator is no longer supported. One should use regular `match` expression instead.

## Guards ##

In Ela 0.11 guards in bindings and inside `match` patterns should always terminate with `else` clause. For example, the following code was valid in Ela 0.10:

```
let fun x | x > 0 = x
```

However it is not longer valid in Ela 0.11. One should write code like so:

```
fun x | x > 0 = x
      | else  = ...
```

## Mutual recursion ##

In Ela 0.11 all bindings (including top level bindings) are mutually recursive. Also the order of bindings is no longer significant. For example, the following code is valid in Ela 0.11 (but would result in compile time error in Ela 0.10):

```
sum x = x + y
y = 2
```

Also expressions in top level are also processed after bindings. For example, the equivalent code in Ela 0.10 would result in compile time warning and yield a unit:

```
2
x = 3
```

In Ela 0.11 however this code would be compiled without warnings and would yield a number `2`.


## Naming ##
In Ela 0.10 it was possible to use uppercase identifiers for module names. The following code was valid:
```
open Core
Core.map
```
However uppercase identifiers in Ela are reserved for variant tags and modules in Ela are first class values. It could lead to
a non-obvious code like so:
```
fun Core
```
What is going on here? Are we applying `fun` function to a variant or to a module reference?

In Ela 0.11 use of uppercase identifiers for module names in disallowed.