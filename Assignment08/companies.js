'use strict'

const mongodb = require('mongodb');
const MongoClient = mongodb.MongoClient;
const mongoUrl = 'mongodb://localhost:27017/app'; //Connection string

//We'll allow the user to pass in a query (or empty query if no condition)
const getCompanies = function(query, cb) {
	MongoClient.connect(mongoUrl, (err, db) => {
		if (err) {
			db.close();
			cb(err);
			return;
		}

		const collection = db.collection('companies');
		collection.find(query).toArray(function(err, docs) {
			db.close();
			cb(null, docs);
		});
	});
};


//Köllum í cb þegar við erum búnir að add-a þessu inn.. 
//cb skilar einhverjum status code
const addCompany = function(company, cb) {
	MongoClient.connect(mongoUrl, (err, db) => {
		if (err) { //if we cannot connect
			db.close();
			cb(err);
			return; 
		}

		const collection = db.collection('companies');
		collection.insert(company, function(ierr, res) {
			if (ierr) {
				db.close();
				cb(ierr);
				return;
			}

			//If no error, we call our callback with an array of created users.. will have one element.. array called "ops", then we have insertedCount and insertedIds in an array.
			db.close();
			cb(null, res); 
		});
	});
};

//Export the function outside of this js file, others can then require this.
module.exports = {
	addCompany: addCompany,
	getCompanies: getCompanies,
};