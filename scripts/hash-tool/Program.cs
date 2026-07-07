using System;
var hash = BCrypt.Net.BCrypt.HashPassword(args[0], 12);
Console.WriteLine(hash);
