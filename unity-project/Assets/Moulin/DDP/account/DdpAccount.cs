/*
	The MIT License (MIT)

	Copyright (c) 2016 Vincent Cantin (user "green-coder" on Github.com)

	Permission is hereby granted, free of charge, to any person obtaining a copy of
	this software and associated documentation files (the "Software"), to deal in
	the Software without restriction, including without limitation the rights to
	use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
	of the Software, and to permit persons to whom the Software is furnished to do
	so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.
*/

﻿using UnityEngine;
using System;
using System.Text;
using System.Threading.Tasks;
#if WINDOWS_UWP
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
#else
using System.Security.Cryptography;
#endif
using System.Collections;

namespace Moulin.DDP {

	public class DdpAccount {

		private DdpConnection connection;

		public bool isLogged;
		public string userId;
		public string token;
		public DateTime tokenExpiration;

		public DdpError error;

		public DdpAccount(DdpConnection connection) {
			this.connection = connection;
		}

		private JSONObject GetPasswordObj(string password) {
#if WINDOWS_UWP
            // Convert the message string to binary data.
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(password, BinaryStringEncoding.Utf8);

            // Create a HashAlgorithmProvider object.
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);

            // Demonstrate how to retrieve the name of the hashing algorithm.
            string strAlgNameUsed = objAlgProv.AlgorithmName;

            // Hash the message.
            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);

            // Verify that the hash length equals the length specified for the algorithm.
            if (buffHash.Length != objAlgProv.HashLength)
            {
                throw new Exception("There was an error creating the hash");
            }

            // Convert the hash to a string (for display).
            string digest = CryptographicBuffer.EncodeToHexString(buffHash);
#else
            string digest = BitConverter.ToString(
				new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(password)));
#endif
            digest = digest.Replace("-", "").ToLower();

            JSONObject passwordObj = JSONObject.Create();
			passwordObj.AddField("digest", digest);
			passwordObj.AddField("algorithm", "sha-256");

			return passwordObj;
		}

		private void HandleLoginResult(MethodCall loginCall) {
			error = loginCall.error;

			if (error == null) {
				JSONObject result = loginCall.result;
				isLogged = true;
				this.userId = result["id"].str;
				this.token = result["token"].str;
				this.tokenExpiration = result["tokenExpires"].GetDateTime();
			}
		}

		private void HandleLogoutResult(MethodCall logoutCall) {
			error = logoutCall.error;

			if (error == null) {
				isLogged = false;
				this.userId = null;
				this.token = null;
				this.tokenExpiration = default(DateTime);
			}
		}

		public void CreateUserAndLogin(string username, string password) {
			JSONObject loginPasswordObj = JSONObject.Create();
			loginPasswordObj.AddField("username", username);
			loginPasswordObj.AddField("password", GetPasswordObj(password));

			MethodCall loginCall = connection.Call("createUser", loginPasswordObj);
            loginCall.OnResult += HandleLoginResult;
		}

		public void Login(string username, string password) {
			JSONObject userObj = JSONObject.Create();
			userObj.AddField("username", username);

			JSONObject loginPasswordObj = JSONObject.Create();
			loginPasswordObj.AddField("user", userObj);
			loginPasswordObj.AddField("password", GetPasswordObj(password));

			MethodCall loginCall = connection.Call("login", loginPasswordObj);
            loginCall.OnResult += HandleLoginResult;
		}

		public void ResumeSession(string token) {
			JSONObject tokenObj = JSONObject.Create();
			tokenObj.AddField("resume", token);

			MethodCall loginCall = connection.Call("login", tokenObj);
            loginCall.OnResult += HandleLoginResult;
		}

		public void Logout() {
			MethodCall logoutCall = connection.Call("logout");
			HandleLogoutResult(logoutCall);
		}

	}

}
