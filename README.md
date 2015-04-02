# REST SDK for Zimbra Social
##Social SDK for REST and OAuthentication
###System Requirements
- Zimbra Social 8.0 or higher
- .NET Framework version 4.5

###Executing the Tests
To execute all the tests you must modify some items. 
- In your Community site, create an Oauth Client that is confidential using Client Credentials
- In the test project, locate Setup.cs in the root
- At the top of th file modify the Url, user and Oauth variables to be appropriate to your site.

>Note that these tests are meant to test the communication infrastrature and SDK logic, not the REST Apis themselves.  If you are having issues with a specific API not related to the SDK please contact support.

####Where is the documentation?
Please refer to the [wiki section](https://github.com/Telligent/Social-Rest-SDK/wiki/) of this repository.

####How do I report a bug?
You can use the [issues section](https://github.com/Telligent/Social-Rest-SDK/issues/) of this repository to report any issues.

####Where can I ask questions?
Please visit our [developer community](http://community.zimbra.com/developers/f) to ask questions, get answers, collaborate and connect with other developers. Plus, give us feedback there so we can continue to improve these tools for you.

####Can I contribute?
Yes, we will have more details soon on how you can contribute.
