Tangent
=======

An experimental programming language, focused on the use of type information to infer order of operations and allow cleaner extensible syntax.
Follow the language blog at http://tangent-lang.blogspot.com/

To get a Tangent compiler, go into the Tangent.Cli project, and build it. 
This will give you a `Tangent.Cli.exe` program in `/bin/Debug` which can be fed tangent file (ending in .tan) and will produce .NET/Mono executables.

If you want a very basic example, you can use `Tangent.Cli.exe moo.tan`, using `moo.tan` from the `Tangent.Cli.Tests` folder. This produces a `moo.exe` that can be executed, and will print "moo."
