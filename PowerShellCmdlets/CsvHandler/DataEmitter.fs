module DataEmitter

open System
open System.Reflection
open System.Reflection.Emit

let assemblyName = new AssemblyName("Dynamics")

let private assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave)

let private moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll")

let private typeBuilder typeName = moduleBuilder.DefineType(typeName, TypeAttributes.Public)

let private fieldBuilder (typeBuilder:TypeBuilder) name fieldType : FieldBuilder = 
    typeBuilder.DefineField(name, fieldType, FieldAttributes.Public)

let private createConstructor (typeBuilder:TypeBuilder) typeList =
    typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, typeList |> List.toArray)

let private toType (name, _) = (name, (typeof<String>))

let emitNewInstanceRef (gen : ILGenerator) =
    gen.Emit(OpCodes.Ldarg_0)
    let objType = typeof<obj>
    gen.Emit(OpCodes.Call, objType.GetConstructor(Type.EmptyTypes))
    gen.Emit(OpCodes.Ldarg_0)

let private loadConstructorArg (gen : ILGenerator) ((num, field) : int * FieldBuilder) = 
    printfn "loading argument %d" num
    gen.Emit(OpCodes.Ldarg, num)
    gen.Emit(OpCodes.Stfld, field)

let private completeConsructor (gen : ILGenerator) = gen.Emit(OpCodes.Ret)
    
let private build (fields : FieldBuilder list) (cons : ConstructorBuilder) = 
    let generator = cons.GetILGenerator()
    
    generator |> emitNewInstanceRef

    fields 
        |> List.zip [1..(List.length fields)]
        |> List.map (loadConstructorArg generator)
        |> ignore

    generator |> completeConsructor

        
let make name types = 
    let typeBuilder = typeBuilder name
    let fieldBuilder = fieldBuilder typeBuilder
    let createConstructor = createConstructor typeBuilder
    let nameType = types |> List.map toType
    let fields = nameType |> List.map (fun (name, ``type``) -> fieldBuilder name ``type``)
    let definedConstructor = nameType |> List.map snd |> createConstructor
    
        
    definedConstructor |> build fields

    typeBuilder.CreateType()


(*
let t = make "foo" [("test", "type")]

let instance = Activator.CreateInstance(t, "value")

let fi = t.GetField("test")

fi.GetValue(instance, null)
*)
(*
        ILGenerator ctor1IL = ctor1.GetILGenerator();
        // For a constructor, argument zero is a reference to the new 
        // instance. Push it on the stack before calling the base 
        // class constructor. Specify the default constructor of the  
        // base class (System.Object) by passing an empty array of  
        // types (Type.EmptyTypes) to GetConstructor.
        ctor1IL.Emit(OpCodes.Ldarg_0);
        ctor1IL.Emit(OpCodes.Call, 
            typeof(object).GetConstructor(Type.EmptyTypes));
        // Push the instance on the stack before pushing the argument 
        // that is to be assigned to the private field m_number.
        ctor1IL.Emit(OpCodes.Ldarg_0);
        ctor1IL.Emit(OpCodes.Ldarg_1);
        ctor1IL.Emit(OpCodes.Stfld, fbNumber);
        ctor1IL.Emit(OpCodes.Ret);
    *)