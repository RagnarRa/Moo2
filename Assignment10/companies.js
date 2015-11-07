'use strict'

const mongodb = require('mongodb');
const MongoClient = mongodb.MongoClient;
const mongoUrl = 'mongodb://localhost:27017/app'; //Connection string
const ObjectID = require('mongodb').ObjectID;

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

const updateCompany = function(id, company, cb) {
	MongoClient.connect(mongoUrl, (err, db) => {
		if (err) {
			db.close();
			cb(err);
			return;
		}
		
		const collection = db.collection('companies');
		collection.update({'_id' : new ObjectID(id)}, company, undefined, function (ierr, result) {
			if (ierr) {
				db.close(); 
				cb(ierr);
				return;
			}
			
			cb(null, result); 
			db.close();
		}); //query and replacement object, and callback..  	
	});
};

const deleteCompany = function(id, cb) {
	MongoClient.connect(mongoUrl, (err, db) => {
		if (err) {
			db.close();
			cb(err);
			return;
		}
		
		const collection = db.collection('companies');
		collection.remove({ '_id' : new ObjectID(id) }, function(ierr, res) {
			if (err) {
				db.close();
				cb(ierr);
				return;
			}
			
			cb(null, res);
			db.close();
		});
		//collection.remove({'_id' : new ObjectID(id)});
	});
};

//Export the function outside of this js file, others can then require this.
module.exports = {
	addCompany: addCompany,
	getCompanies: getCompanies,
	updateCompany: updateCompany,
	deleteCompany: deleteCompany
};