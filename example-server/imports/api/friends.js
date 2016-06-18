import { Meteor } from 'meteor/meteor';
import { Mongo } from 'meteor/mongo';
import { check } from 'meteor/check';

export const Friends = new Mongo.Collection('friends');

Meteor.methods({
  'friends.create'(name) {
    check(name, String);

    // Create the record.
    Friends.insert({
      name: name,
    });
  },
  'friends.removeAll'() {
    Friends.remove({});
  },
  'friends.add'(a, b) {
    return a + b;
  }
});

if (Meteor.isServer) {
  Meteor.publish('friends', function() {
    return Friends.find({});
  });
}
