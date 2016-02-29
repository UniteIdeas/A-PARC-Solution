# README #

This README would normally document whatever steps are necessary to get your application up and running.

### What is this repository for? ###

* Developers

### How do I get set up? ###

* Open the file "TextClassification.sln" with Visual Studio 2013.
* Open the file "Program.cs" in the "TextClassification" project.
* In program.Main() insert the following code:

	var filePath = "c:/path/to/file.txt";
	//Get the classified training data
	var bags = bagsOfWordsService.GetBagOfWords(@"",reload: false);
	
	//Read the input file
	var textService = new TextService();
	var sentences = textService.Read(filePath);
	
	//Classify the data
    var classifierTweaked = new ClassifierTFIDFTweakedService();
    var predictions = classifierTweaked.Classify(sentences, bags);
    
	//Output is a html file, it gets stored in the same folder as the input
	var resultLocation = String.Format("{0}\\{1}{2}", Path.GetDirectoryName(filePath)
                    , Path.GetFileNameWithoutExtension(filePath), "_out.html");
	var html = classifierTweaked.GetHtml(predictions, System.IO.Path.GetFileName(filePath));
	File.WriteAllText(resultLocation, html);
	
* Change the filePath variable to the path of the input file.
* The output will be written in a HTML file as: "<filename>_out.html".

### Contribution guidelines ###

* Writing tests
* Code review
* Other guidelines

### Who do I talk to? ###

* Repo owner or admin: Wouter Eekhout w.t.k.eekhout@fgga.leidenuniv.nl