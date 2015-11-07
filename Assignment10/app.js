'use strict'

const express = require('express');
const bodyParser = require('body-parser');

const app = express();
app.use(bodyParser.json());

const elasticsearch = require('elasticsearch');
const elasticClient = new elasticsearch.Client({
	host: 'localhost:9200',
	log: 'error'
});

const companies = require('./companies');
const ObjectID = require('mongodb').ObjectID;
const ADMIN_TOKEN = '2632dcfd-ed04-4438-885f-bdf9450c5af8';

/*
app.get("/api//feeds/:wall_id", (req, res) => {
	const page = req.query.page || 0; 
	const size = req.query.size || 10; 
	
	// /index/type/id (id optional)
	const promise = elasticClient.search({
		'index' : 'feeds',
		'type' : 'feed',
		'size' : size,
		'from' : page,
		'body' : {
			'query' : {
				"match": {
					"wall_id" : "32f4......"
				}
			}
		}
	});
	
	promise.then((doc) => {
		res.send(doc.hits.hits.map( (d) => d._source )); //Takes the object d.. and maps it to d._source
	}, (err) => {
		res.status(500).send(err); 
	});
});

app.post('/api/feeds/:wall_id', bodyParser.json(), (req, res) => {
	//Add to DB, if success.. add to ES
	
	const data = {
		"wall_id" : "4d467...",
		"author_id" : "etc",
	};
	
    const promise = elasticClient.index({
		'index' : 'feeds',
		'type' : 'feed',
		'id' : 1, //would be same as in db
		'body' : data
	});
	
	promise.then((doc) => {
		res.send(doc); 
	}, (err) => {
		res.status(500).send(err); 
	});
}); */
	

//POSTs a company, needs admin authentication.. 
/* 
{     id: Unique id for the company, title: String representing the company name, description: String represetning description for the company,  
	  url: String representing the company homepage, created: Date representing the creation date of the company (when it was posted) 
} 

This endpoint should be authorized with a token named ADMIN_TOKEN. ** 

If ADMIN_TOKEN is missing/incorrect => 401 Unauthorized ** 

If content-type in the request header is not application/json => 415 Unsupported Media Type. ** 

If a company with the same name exists, then we answer with status code 409 Conflict. **

If preconditions are met then the company should be written to MongoDB AND to ElasticSearch => 201. ** 

Note that the id of the document in ElasticSearch should contain the same id as the document within MongoDB. 
This endpoint should then answer with status code 201
and the response body should include a Json document named id
and should have id of the newly created company as value.
*/
app.post('/api/company', (req, res) => {
	const headers = req.headers;

	if (!headers.hasOwnProperty('admin_token') || headers.admin_token !== ADMIN_TOKEN) {
		res.status(401).send('Invalid authentication.');
		return; 
	}
	
	//Make sure the request has content-type application/json 
	if (!headers.hasOwnProperty('content-type') || headers["content-type"] !== 'application/json') {
		res.status(415).send('Must have the content-type application/json.');
		return;
	}
	
	const data = req.body;
	if (!data.hasOwnProperty('title') || 
		!data.hasOwnProperty('description') ||
		!data.hasOwnProperty('url')) {
		res.status(412).send('Company does not have all the required properties.');
		return; 
	}
	
	data.created = new Date(); 

	if (!(typeof data.title === "string"))  {
		res.status(412).send('Company title must be a string.');
		return;
	}
	else if (data.title.length === 0) {
		res.status(412).send('Company title must not be empty.');
		return;
	}

	if (!(typeof data.description === "string"))  {
		res.status(412).send('Company description must be a string.');
		return;
	}
	else if (data.description.length === 0) {
		res.status(412).send('Company description must not be empty.');
		return;
	}
	
	//Verify that no company with this company's name exists
	companies.getCompanies({ 'title' : data.title }, function(err, docs) {
		if (docs.length > 0) {
			res.status(409).send('This company title is already taken.');
			return; 
		}
		
		companies.addCompany(data, (err, dbrs) => {
			if (err) {
				res.status(500).send('Unable to insert company.'); //500 Internal Server Error
				return; 
			}
			
			//TODO: Use Kafka to send to ElasticSearch (even though not specified in assignment)
			
			var documentID = dbrs.insertedIds[0].toString();
			
			data._id = data._id.toString();
			console.dir(data); 
			
			const promise = elasticClient.index({
				'index' : 'companies',
				'type' : 'company',
				'id' : documentID, //DocumentID 
				'body' : data
			});
			
			promise.then((doc) => {
				res.status(201).json({"id" : dbrs.insertedIds[0] } || ''); 
			}, (err) => {
				res.status(500).send(err); 
			}); 
		});
	});
});

/* Endpoint for fetching list of companies that have been added to Punchy.
The companies should be fetched from ElasticSearch.
 This endpoint should return a list of Json objects with the following fields.

id, title, description, url

Other fields should be EXCLUDED. This endpoint accepts two request parameters, page and max.
 If they are not presented they should be defaulted by 0 and 20 respectively.
 They should control the pagination in Elasticsearch
 and allow the client to paginate the result.
 The list should be ordered by alphabetically by the company title. */

app.get('/api/company', (req, res) => {
	const page = req.query.page || 0; 
	const max = req.query.max || 20; 
	
	// /index/type/id (id optional)
	const promise = elasticClient.search({
		'index' : 'companies',
		'type' : 'company',
		'size' : max, //Number of results to return
		'from' : page, //Page x 
		'body' : { 
			'_source' : ["_id", "title", "description", "url"],
			'query' : {
				'match_all' : {}
			},
			"sort" : [
				 { "title" : { "order" : "asc" } }
			]
		} 
	});
	
	promise.then((doc) => {
		console.dir(doc.hits.hits);
		res.send(doc.hits.hits.map( (d) => d._source ));
		
		/*
		res.send(doc.hits.hits.map( (company) => {
			return {
				//"_id": company._source._id,
				"title": company._source.title,
				"description": company._source.description,
				"url": company.fields.url
			}
		})); */
	}, (err) => {
		res.status(500).send(err); 
	});
});

app.get('/api/company/:id', (req, res) => {
	const id = req.params.id; 
	if (!ObjectID.isValid(id)) {
		res.status(412).send('Invalid company id.');
		return;
	}
	
	companies.getCompanies({ '_id' : new ObjectID(id) }, function(err, docs) {
		if (docs.length > 0) {
			delete docs[0].created;
			res.json(docs[0]);
		} else {
			res.status(404).send('No company found with that id!');
		}
	});
});

//Update the given company 
//Should be PUT? 
//It would be elegant to put the company object validation into some middleware, since it's shared by this update guy
//And the post guy.. maybe after the exams. Or even use some Mongoose validation. 
app.post('/api/company/:id', (req, res) => {
	const id = req.params.id;
	if (!ObjectID.isValid(id)) {
		res.status(412).send('Invalid company id.');
		return;
	}
	
	const headers = req.headers;

	if (!headers.hasOwnProperty('admin_token') || headers.admin_token !== ADMIN_TOKEN) {
		res.status(401).send('Invalid authentication.');
		return; 
	}
	
	//Make sure the request has content-type application/json 
	if (!headers.hasOwnProperty('content-type') || headers["content-type"] !== 'application/json') {
		res.status(415).send('Must have the content-type application/json.');
		return;
	}
	
	const updatedCompany = req.body;
	if (!updatedCompany.hasOwnProperty('title') || 
		!updatedCompany.hasOwnProperty('description') ||
		!updatedCompany.hasOwnProperty('url')) {
		res.status(412).send('Company does not have all the required properties.');
		return; 
	}		
	
	if (!(typeof updatedCompany.title === "string"))  {
		res.status(412).send('Company title must be a string.');
		return;
	}
	else if (updatedCompany.title.length === 0) {
		res.status(412).send('Company title must not be empty.');
		return;
	}

	if (!(typeof updatedCompany.description === "string"))  {
		res.status(412).send('Company description must be a string.');
		return;
	}
	else if (updatedCompany.description.length === 0) {
		res.status(412).send('Company description must not be empty.');
		return;
	}
	
	companies.getCompanies({ '_id' : new ObjectID(id) }, function(err, docs) {	
		if (docs.length > 0) { //The company exists
			updatedCompany.created = docs[0].created; //Will not change.. probably.. 
			companies.updateCompany(id, updatedCompany, (err, dbrs) => {
				if (err) {
					res.status(500).send('Unable to update company.'); //500 Internal Server Error
					return; 
				}
				
				updatedCompany._id = id; 
				
				//Index again will update the index
				const promise = elasticClient.index({
					'index' : 'companies',
					'type' : 'company',
					'id' : id, //DocumentID 
					'body' : updatedCompany
				});
			
				promise.then((doc) => {
					res.send("Success"); 
				}, (err) => {
					res.status(500).send(err); 
				}); 
				
				//res.send("Success!"); //Defaults to 200 
			});
		} else {
			res.status(404).send('No company found with that id!');
		}
	});
});

app.delete('/api/company/:id', (req, res) => {
	const headers = req.headers;
	if (!headers.hasOwnProperty('admin_token') || headers.admin_token !== ADMIN_TOKEN) {
		res.status(401).send('Invalid authentication.');
		return; 
	}
	
	const id = req.params.id;
	if (!ObjectID.isValid(id)) {
		res.status(412).send('Invalid company id.');
		return;
	}
	
	companies.getCompanies({ '_id' : new ObjectID(id) }, function(err, docs) {	
		if (docs.length > 0) { //The company exists
			//Remove it from MongoDB
			companies.deleteCompany(id, (err, dbrs) => {
				if (err) {
					res.status(500).send('Unable to update company.'); //500 Internal Server Error
					return; 
				}
				
				//Remove it from ElasticSearch
				const promise = elasticClient.delete({
					'index' : 'companies',
					'type' : 'company',
					'id' : id, //DocumentID 
				});
				
				promise.then((doc) => {
					res.send("Company removed."); 
				}, (err) => {
					res.status(500).send(err); 
				}); 
			});
			
		} else {
			res.status(412).send("Company doesn't exist.");
		}
	});
});

//Search results shold include id, title, description, url
//Incoming data is { 'search' : search-string } 
//Can be a full-text search
app.post('/api/company/search', (req, res) => {
	const promise = elasticClient.search({
		'index' : 'companies',
		'type' : 'company', 
		'body' : { 
			'_source' : ["_id", "title", "description", "url"], //Get only these fields
			'query' : {
				'match_all' : {}
			}
		} 
	});
	
	promise.then((doc) => {
		res.send(doc.hits.hits.map( (d) => d._source ));
	}, (err) => {
		res.status(500).send(err); 
	});
});

app.listen(4000, () => { 
	console.log('Server starting on port 4000'); 
}); 