Tangent
=======

An experimental programming language, focused on the use of type information to infer order of operations and allow cleaner extensible syntax.
Follow the language blog at http://tangent-lang.blogspot.com/

To get a Tangent compiler, go into the Tangent.Cli project, and build it. 
This will give you a `Tangent.Cli.exe` program in `/bin/Debug` which can be fed a tangent file (ending in .tan) and will produce .NET/Mono executables.

If you want a very basic example, you can use `Tangent.Cli.exe moo.tan`, using `moo.tan` from the `Tangent.Cli.Tests` folder. This produces a `moo.exe` that can be executed, and will print "moo."

### Intro Guide

```
// Comments in Tangent start with //. /**/ style comments are not supported.

// To have a complete Tangent program, you need an entrypoint.
entrypoint => void {}
```

```
// In the current version, there is a basic print command, which works with the common types.
entrypoint => void {
  print "Hello World!";
  print 42;
  print true;
} 
```

```
// Local variables are shaped like :name:type:=initializer
entrypoint => void {
  : x : int := 42;
  print x;            // 42
  x = x + 1;
  print x;            // 43;
}
```

```
// Basic math operations exist. Division and modulo are not implemented yet.
entrypoint => void {
  print 4 + 4;
  print 4 - 2;
  print 4 * 4;
  print 4 < 4;
  print 4 = 4;
  print true and false;
  print true or false;
  
  // Also, beware that Tangent doesn't respect math operation precedence yet!
  print 4 + 2 * 4;  // 24.
}
```

```
// functions are declared with =>
HelloWorld => string {
  // the value returned by a function is simply the last line of the function.
  "Hello World!";
}

entrypoint => void {
  print HelloWorld;
}
```

```
// parameters are placed in parens, in the form name:type
say (words : string) => void {
  print words;
}

entrypoint => void {
  say "Hello";
  say "World!";
}
```

```
// Tangent though doesn't require such rigid function structure
say (words : string) to (username : string) => void {
  print "Dear:";
  print username;
  print "";
  print words;
}

entrypoint => void {
  say "Hello" to "World";
}
```

```
// Function names need not be words at all
(source location : string) -> (destination : string) => void {
  print source location;
  print " to";
  print destination;
}

entrypoint => void {
  "New York" -> "Boston";
}
```

```
// Tangent supports enums
elements :> enum {
  Hydrogen,
  Helium,
  Lithium // and so on.
}

// which can then be specialized in functions
(h : elements.Hydrogen) => string { "H" }      // note that the semi-colon is optional in the last statement
(he : elements.Helium) => string { "He" }      // and that conversion functions lack identifiers
(li : elements.Lithium) => string { "Li" }
(_ : elements) => string { "unknown" }

entrypoint => void {
  print Lithium;
}
```

```
// Tangent supports classes
//
// They look like type-name :> constructor { body }
point :> (x : int), (y : int) {
  (this).x : int := x;       // fields can access constructor arguments
  (this).y : int := y;       // require a (this) argument, a type, and an initializer.
}

// Functions don't need to be declared in the class
(a : point) = (b : point) => bool {
  a.x = b.x and a.y = b.y
}

entrypoint => void {
  print 2,4 = 2,4;
  print 2,4 = 4,2;
}
```

```
// For read only fields, just use functions
point :> (x : int), (y : int) {
  (this).x => int { x }       
  (this).y => int { y }       
}

entrypoint => void {
  : pt : point := 4,2;
  print pt.x;
}
```

```
// Tangent supports generics
pair (T) :> (x : T), (y : T) {
  (this).x : T := x;       
  (this).y : T := y;       
}

// Tangent also supports generic inference. 
// (T) here says "please infer the type argument of a, and assign it to T"
// b then is required to be a pair of that type.
(a : pair (T)) = (b : pair T) => bool {
  a.x = b.x and a.y = b.y
}

entrypoint => void {
  print 2,4 = 2,4;
  print "foo", "bar" = "foo", "bar";
}
```

```
// The lazy arrow ~> makes something like C#'s Func<T> or Action
entrypoint => void {
  : x : int := 42;
  : immediate : bool := x < 100;
  : lazy : ~> bool := x < 100;
  
  print immediate;  // true
  print lazy;       // true
  
  x = 256;
  
  print immediate;  // true
  print lazy;       // false
}
```

```
// It works in parameters too
// Combined with specialization and dynamic dispatch, you get conditionals.

if (condition : bool) (when true : ~> void) else (when false : ~> void) => void { when false }
if (condition : bool.true) (when true : ~> void) else (when false : ~> void) => void { when true }  

entrypoint => void {
  : x : bool := true;
  
  if x print "x" else print "y";      // x
  x = false;
  if x print "x" else print "y";      // y
}
```

```
// Tangent has interfaces.

// You declare them similarly to enums or classes:
comparable :> interface {
  (this) < (this) => bool;
}

// interfaces are implemented using a weird operator that looks like a duck :<
// They can be implemented inline:
bushel of apples :> (number of apples : int) apples :< comparable {
  number of apples in (this) => int { number of apples }
  (this) < (that : bushel of apples) => bool { number of apples in this < number of apples in that }
  (this) = (that : bushel of apples) => bool { number of apples in this = number of apples in that }
}

// or, if you have an existing class
size :> enum {
  small,
  large
}

// you can implement the interface explicitly:
size :< comparable { 
  (this) < (that : size) => bool { this = small and that = large }
}

// They can then be used as generic constraints
(a : T : comparable) is less than (b : T) => bool { a < b }

entrypoint => void {
  print small is less than large;
  print 4 apples is less than 2 apples;
}
```
