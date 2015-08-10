# This project has moved to [CodePlex](http://ela.codeplex.com) #

---


# Ela Programming Language #

<img src='http://www.elalang.net/includes/elide_screen2.png' align='right' border='0' />

Ela is a modern programming language that runs on CLR and [Mono](http://www.mono-project.com).

Ela is a pure functional language with dynamic (and strong) typing. It provides an extensive support for the functional programming paradigm including but not limited to - first class functions and modules, curried function application, pattern matching, algebraic types, type classes, and much more. It also provides a support for both strict and non-strict evaluation. (See [Ela distinctive features](ElaDistinctiveFeatures.md)).

The current language implementation is a light-weight and efficient virtual machine written fully in C#. Ela was designed to be embeddable and has a clear and straightforward API.

_Want to know more? Read a_<a href='http://elalang.net/ElaBook.aspx'>book about Ela</a>_._

Now Ela is distributed as a part of <a href='https://ela.codeplex.com/releases'>Ela Platform</a> (no installation required) that includes Ela, Ela interactive environment (REPL), documentation library, code samples, Ela standard library and Elide, a graphical development environment.

## Code samples in Ela ##

Sieve of Eratosthenes:

```
primes xs = sieve xs
  where sieve [] = []
        sieve (p::xs) = 
          & p :: sieve [& x \\ x <- xs | x % p > 0]

//Outputs: [2,3,5,7] 
primes [2..10]
```

Type classes:

```
//Class with a function overloaded by return type
class Pointed a where
  point _->a 

//Instance for a linked list
instance Pointed List where
  point x = [x] 
  
point 42 ::: List //Outputs: [42]
```

Infinite list filtering:

```
lst = [1,4..]

filter _ [] = []
filter p (x::xs) 
  | p x  = & x :: filter p xs
  | else = filter p xs

nlst = filter (>10) lst
```

Lazy fix point combinator:

```
fix f = f (& fix f)

factabs fact 0 = 1
factabs fact x = x * fact (x - 1)

res = (fix factabs) 5
```

_[See more](http://code.google.com/p/elalang/wiki/CodeSamples)_