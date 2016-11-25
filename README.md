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

