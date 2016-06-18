import { Meteor } from 'meteor/meteor';
import { Template } from 'meteor/templating';
import { ReactiveDict } from 'meteor/reactive-dict';

import { Friends } from '../api/friends.js';

import './friend.js';
import './body.html';

Template.body.onCreated(function bodyOnCreated() {
  this.state = new ReactiveDict();
  Meteor.subscribe('friends');
});

Template.body.helpers({
  friends() {
    return Friends.find();
  }
});
