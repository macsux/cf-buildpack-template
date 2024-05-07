namespace CloudFoundry.Buildpack.V2.Build;
// See reference: https://minnie.tuhs.org/cgi-bin/utree.pl?file=4.4BSD/usr/include/sys/stat.h
// (values are in octal - use Convert.ToInt32("OCTALNUM", 8) to get values you see below
[Flags]
enum ZipEntryAttributes
{
    ExecuteOther = 1,
    WriteOther = 2,
    ReadOther = 4,
	
    ExecuteGroup = 8,
    WriteGroup = 16,
    ReadGroup = 32,

    ExecuteOwner = 64,
    WriteOwner = 128,
    ReadOwner = 256,

    Sticky = 512, // S_ISVTX
    SetGroupIdOnExecution = 1024,
    SetUserIdOnExecution = 2048,

    //This is the file type constant of a block-oriented device file.
    NamedPipe = 4096,
    CharacterSpecial = 8192,
    Directory = 16384,
    Block = 24576,
    Regular = 32768,
    SymbolicLink = 40960,
    Socket = 49152
	
}