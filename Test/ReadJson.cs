using System;
using Newtonsoft.Json;
using System.IO;

public class Class1
{
	public Class1()
	{
	}

	static void Main(string[] args)
	{
		string jsonPath = @"C:\Users\Utilisateur\Desktop\Projet 1\fichier json\BiZiiPAD_20221014_79394";
		string json = File.ReadAllText(jsonPath);
		//conversion du json en Objet c#
		Console.WriteLine(json);
		var jsonObject = JsonConvert.DeserializeObject<String>(json);

	}
}
