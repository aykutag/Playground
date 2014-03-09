namespace CsvHandler

open System
open System.Reflection
open System.Reflection.Emit

module DataEmitter = 
    type DynamicField = {
        Name : String;
        Type : Type;
        Value: obj;
    }

    let private assemblyName = new AssemblyName("Dynamics")

    let private assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave)

    let private moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll")

    let private typeBuilder typeName = moduleBuilder.DefineType(typeName, TypeAttributes.Public)

    let private fieldBuilder (typeBuilder:TypeBuilder) name fieldType : FieldBuilder = 
        typeBuilder.DefineField(name, fieldType, FieldAttributes.Public)

    let private createConstructor (typeBuilder:TypeBuilder) typeList =
        typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, typeList |> List.toArray)

    let private toType (name, _) = (name, (typeof<String>))

    let private callDefaultConstructor (gen: ILGenerator) = 
        let objType = typeof<obj>
        gen.Emit(OpCodes.Call, objType.GetConstructor(Type.EmptyTypes))
        gen.Emit(OpCodes.Ldarg_0)

    let private loadThis (gen: ILGenerator) = 
        gen.Emit(OpCodes.Ldarg_0)
        gen

    let private emitNewInstanceRef (gen : ILGenerator) =
        gen |> loadThis |> callDefaultConstructor

    let private assignField (argIndex : int) (field : FieldBuilder) (gen : ILGenerator) =        
        gen.Emit(OpCodes.Ldarg, argIndex)
        gen.Emit(OpCodes.Stfld, field)
        gen

    let private loadConstructorArg (gen : ILGenerator) ((num, field) : int * FieldBuilder) = 
        gen |> loadThis |> assignField num field

    let private completeConsructor (gen : ILGenerator) = gen.Emit(OpCodes.Ret)
    
    let private build (fields : FieldBuilder list) (cons : ConstructorBuilder) = 
        let generator = cons.GetILGenerator()
    
        generator |> emitNewInstanceRef

        let fieldsWithIndexes = fields |> List.zip [1..(List.length fields)]

        fieldsWithIndexes
            |> List.map (loadConstructorArg generator)
            |> ignore

        generator |> completeConsructor

        
    let make name types = 
        let typeBuilder = typeBuilder name
        let fieldBuilder = fieldBuilder typeBuilder
        let createConstructor = createConstructor typeBuilder        
        let fields = types |> List.map (fun (name, ``type``) -> fieldBuilder name ``type``)
        let definedConstructor = types |> List.map snd |> createConstructor
    
        
        definedConstructor |> build fields

        typeBuilder.CreateType()

    let instantiate typeName objInfo =
        let values = objInfo |> List.map (fun i -> i.Value) |> List.toArray
        let types  = objInfo |> List.map (fun i -> (i.Name, i.Type))

        let t = make typeName types

        Activator.CreateInstance(t, values)