using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program
{
    static void Main()
    {
        var bTree = new BTree(3); //Árbol B con grado 3

        //Leer los libros
        var booksPath = @"/Users/harriett/Downloads/lab01_books.csv";
        var lines = File.ReadAllLines(booksPath);
        foreach (var line in lines)
        {
            var parts = line.Split(';');
            var operation = parts[0];
            var json = parts[1];

            switch (operation)
            {
                case "INSERT":
                    var book = JsonConvert.DeserializeObject<Book>(json);
                    bTree.Insert(book);
                    break;
                case "PATCH":
                    var updates = JObject.Parse(json);
                    var isbn = updates["isbn"].ToString();
                    bTree.Update(isbn, updates);
                    break;
                case "DELETE":
                    var deleteBook = JsonConvert.DeserializeObject<Book>(json);
                    bTree.Delete(deleteBook.Isbn);
                    break;
            }
        }

        //leer el segundo archivo
        var searchPath = @"/Users/harriett/Downloads/lab01_search.csv";
        var searchLines = File.ReadAllLines(searchPath);
        var results = new List<Book>();
        foreach (var line in searchLines)
        {
            var parts = line.Split(';');
            var operation = parts[0];
            var json = parts[1];

            switch (operation)
            {
                case "SEARCH":
                    var searchParams = JObject.Parse(json);
                    var name = searchParams["name"].ToString();
                    var searchResults = bTree.SearchByName(name);
                    results.AddRange(searchResults);
                    break;
            }
        }

        //los resultados (resultados)
        var resultPath = @"/Users/harriett/Downloads/lab01_result.txt";
        using (var writer = new StreamWriter(resultPath))
        {
            foreach (var result in results)
            {
                writer.WriteLine(JsonConvert.SerializeObject(result));
            }
        }
        
        foreach (var result in results)
        {
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}
