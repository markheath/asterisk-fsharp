module HighScore
    open System.IO

    type HighScore = { Level:int;Name:string}
    let fileName = "record.txt"

    let load () = 
        if File.Exists(fileName) then
            let record = File.ReadAllLines(fileName).[0]
            let [|level;name|] = record.Split [|':'|] 
            {Level=int level;Name=name}
        else
            {Level=0;Name="Nobody"}
    
    let save {Level=level;Name=name} =
        let record = sprintf "%d:%s" level name
        File.WriteAllText(fileName, record)         

    let describe {Level=level;Name=name} = sprintf "Level %d by %s" level name
        