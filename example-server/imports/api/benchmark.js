import { Meteor } from 'meteor/meteor';
import { Mongo } from 'meteor/mongo';
import { check } from 'meteor/check';
import { Match } from 'meteor/check';

let fs = Npm.require('fs');

export const Benchmark = new Mongo.Collection('benchmark');
// amount of entries we use to test
const NUM_ENTRIES = 100;

// cache image data in local variable
let IMAGE = null;

let getImage = function() {
  IMAGE = IMAGE || Assets.getText('test-pattern.txt');
  return IMAGE;
}

function genTestData() {
  return {
    "arr": [1, 2, 3],
    "str": "some string",
    "float": 1.23456789,
    "int": 123456789,
    "null": null,
    "date": new Date(),
    // png image as base-64
    "image": getImage()
  }
}

function getBenchmarkData() {
  let qry = {}
  let bench = Benchmark.find(qry, {limit: NUM_ENTRIES});
  if (bench.count() >= NUM_ENTRIES) {
    return bench;
  }
  for (let i = bench.count(); i < 100; i++) {
    Benchmark.insert({
      "nr": i,
      "values": genTestData()
    })
  }

  return Benchmark.find(qry, {limit: NUM_ENTRIES});
}

Meteor.methods({
  'benchmark.recreate'() {
    // remove everything!
    Benchmark.remove({});
    getBenchmarkData();
    return true;
  },
  'benchmark.count'(i) {
    check(i, Match.Integer);
    return i+1;
  }
});

if (Meteor.isServer) {
  Meteor.publish('benchmark', function() {
    return getBenchmarkData();
  });
}
