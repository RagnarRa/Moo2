var express = require('express');
var app = express(); 
//app.use(express.bodyParser()); //To parse the body of, for an example, a POST request.. we need this middleware. Parses the body of the request and sets the body property on the request object. 
app.use(express.json()); //Since they're deprecating bodyParser, we'll use these two
app.use(express.urlencoded());

var quotes = [
  { author : 'Audrey Hepburn', text : "Nothing is impossible, the word itself says 'I'm possible'!"},
  { author : 'Walt Disney', text : "You may not realize it when it happens, but a kick in the teeth may be the best thing in the world for you"},
  { author : 'Unknown', text : "Even the greatest was once a beginner. Don't be afraid to take that first step."},
  { author : 'Neale Donald Walsch', text : "You are afraid to die, and you're afraid to live. What a way to exist."}
];

app.listen(process.env.PORT || 3412); 

app.get('/', function(req, res) {
	res.json(quotes); //Send a json object with all the quotes back..
});

//Retrieve a random quote.. noteworthy, we have to put the random route first before /quote/:id.. otherwise random would be treated as an id. 
app.get('/quote/random', function(req, res) {
	var id = Math.floor(Math.random() * quotes.length);
	var q = quotes[id];
	res.json(q);
});

//Add a :param to the route.. the param will be available using req.params.. 
app.get('/quote/:id', function(req, res) {
	if (quotes.length <= req.params.id || req.params.id < 0) {
		res.statusCode = 404;
		return res.send('Error 404: No quote found');
	}

	var q = quotes[req.params.id];
	res.json(q);
});


app.post('/quote', function(req, res) {
	if (!req.body.hasOwnProperty('author') || 
		!req.body.hasOwnProperty('text')) {
		res.statusCode = 400;
	return res.send('Error 400: Post syntax incorrect.');
	}

	var newQuote = {
		author: req.body.author,
		text: req.body.text
	};

	quotes.push(newQuote);
	res.json(true); //push back a true response
});

app.delete('/quote/:id', function(req, res) {
	if (quotes.length <= req.params.id) {
		req.statusCode = 404;
		return res.send('Error 404: No quote found');
	}

	quotes.splice(req.params.id, 1);
	res.json(true);
});
