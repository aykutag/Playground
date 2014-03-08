namespace CsvHandler

open System.Management.Automation

type x = { Name : string; Foo: string }

[<Cmdlet("Csv", "Read")>]
type CsvParser() =
    inherit PSCmdlet()
    
    [<Parameter>]
    member val File : string = null with get, set

    override this.ProcessRecord() = 
        
        this.WriteObject({ Name = this.File; Foo = "foo" })