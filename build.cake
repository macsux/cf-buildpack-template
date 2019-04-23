using System.IO;
var target = Argument("target", "Default");

Task("Default")
  .Does(() =>
{
  var dir = System.IO.Directory.CreateDirectory("publish");
     
  Information("Hello World!");
});

RunTarget(target);