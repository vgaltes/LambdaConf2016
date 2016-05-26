#load "./packages/FsLab/FsLab.fsx"

let a = 1
let b = 2

//public int Sum(int a, int b)
//{
//    return a + b;
//}

// Functions
let sum a b = 
    a + b
let res = sum a b

// Tuples
let aTuple = ("LambdaConf", 2016)
let name, year = aTuple
let conferenceYear (_, year) = year
let year' = conferenceYear aTuple
let year'' = snd aTuple
let name' = fst aTuple

// Types
type Conference = {Name: string; Year: int}
let lambdaConf2016 = {Name= "LamdaConf"; Year=2016}
let lambdaConf2017 = {lambdaConf2016 with Year = 2017}

// Partial application
let addOne x = sum x 1
let addTwo x = sum x 2
let res1 = addOne 3
let res2 = addTwo 3

// Pipes
let r = addOne 3
let res1' = 3 |> addOne
let res2' = res1' |> addTwo
let res3 = 3 |> addOne |> addTwo

// Function composition
let addThree = addOne >> addTwo
let res3' = 3|> addThree

// Discriminated unions (and pattern matching)
let s = Some("Hello")
let s' = None

match s' with
| Some(x) -> printfn "%s" x
| _ -> printfn "whatever"

type Figure =
    | Circle
    | Rectangle

let f = Figure.Rectangle

type Figure' =
    | Circle of radius:float
    | Rectangle of width:float * height:float

let f' = Figure'.Rectangle(width = 1.5, height = 5.6)

match f' with
| Circle(r) -> printfn "Circle with radius %f" r
| Rectangle(w, h) -> printfn "Rectangle with width = %f and height = %f" w h

// Collections
let numbers = [|1;2;3;4|]

let numbers' = [|100..120|]

let numbers'' = [|100..3..120|]

let numbersFromExpression = [|for i in 100..120 do
                              yield i * 2|]

// Array.map
// Takes an array and returns another array of the same length with the result of applying 
// a function to each element.
let squares = 
        numbers 
        |> Array.map (fun i -> i * i)


// Array.mapi
// Is very similar to Array.map but it provides the index of each element.

let letters = [|'a';'b';'c';'d'|]
letters |> Array.mapi (fun i l -> sprintf "The letter at index %i is %c" i l)

// Array.iter
// Iterates and call a function with each element, but it doesn’t returns anything 
// (only has side effects). We can user Array.iteri if we need the index.

letters |> Array.iteri (fun i l -> printf "The letter at index %i is %c" i l)

// Array.filter
// Given an array only returns those elements on which the function applied returns true.

let evenNumbers = 
    numbers'
    |> Array.filter (fun n -> n % 2 = 0)
      
// Array.choose
// Given an array only returns those elements on wich the function applied returns a ‘Some’ result. 
// So, the function applied must return an option type.

let evenNumbers' = 
    numbers'
    |> Array.choose (fun n -> if ( n % 2 = 0 ) then Some(n) else None)
  
// Array.sum
// Sum the values of the array. The type of the array must support addition and must have a zero member.

let sumNumbers = 
    numbers
    |> Array.sum
  
// Array.sumBy
// Same as sum but takes a function that select the element to sum.

// Array.sort
// Given an array, returns the array sorted by the element. If we use sortBy, we can specify 
// a function to be used to sort

let sortedNumbers =
    numbers
    |> Array.sort
    
// Array.reduce
// Given an array, uses the supplied function to calculate a value that is used as accumulator 
// for the next calculation. Throws an exception in an empty input list.

let strings = [|"This"; "is"; "a"; "sentence"|]
let sentence =
    strings
    |> Array.reduce (fun acc s -> acc + " " + s)
    
// Array.fold
// Same as reduce, but takes as a parameter the first value of the accumulator.

let sentence' =
    strings
    |> Array.fold  (fun acc s -> acc + " " + s) "Fold:"

// Array.scan
// Like fold, but returns each intermediate result

let sentence'' =
    strings
    |> Array.scan  (fun acc s -> acc + " " + s) "Scan:"
    
// Array.zip
// Takes two arrays of the same size and produce another array of the same size with tuples 
// of elements from each input array.

let colorNames = [|"red";"green";"blue"|]
let colorCodes = [|"FF0000"; "00FF00"; "0000FF"|]
let colors =
    Array.zip colorNames colorCodes

// Array.collect
// For each element of the array, applies the given function. 
// Concatenates all the results and return the combined array.
let a1 = [|1;2;3|]
let a2 = [|11;12|]
let a3 = [|21;22;23;24|]
let at = [|a1;a2;a3|]
let collect = 
    at
    |> Array.collect id
    //|> Array.collect (fun a -> a)
    
// Type providers
open FSharp.Data
type TestCsv = CsvProvider<"FirstName,LastName,City">
let csvFile = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "Data", "test.csv")
let csv = TestCsv.Load csvFile
let ovais = csv.Rows |> Seq.head
let city = ovais.City
