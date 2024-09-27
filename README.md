## Environment:  
- .NET version: 6.0

## Read-Only Files:   
- NewsFeedService.Tests/IntegrationTests.cs
- NewsFeedService.WebAPI/Services/NewsFeedService.cs

## Data:  
Example of a news feed item data JSON object:
```
{
    id: 1,
    title: "news title 1",
    authorName: "Rick D.",
    body: "Advantage old had otherwise sincerity dependent additions.",
    allowComments: true,
    dateCreated: 1573843210
}  
```

## Requirements:

A company is launching a service that can manage news items. You already have a prepared infrastructure and need to implement a Web API Controller NewsFeedController. Also, you need to think about performance. The service endpoint for getting all available news can potentially take a long time in case of large data. Therefore, you need to implement caching for this endpoint using the in-memory caching mechanism from .NET core.



The following APIs is already implemented:

1. Creating news: a POST request to the endpoint `api/newsfeed` adds the news item to the database. The HTTP response code is 200.
2. Getting all news: a GET request to the endpoint `api/newsfeed` returns the entire list of news. The HTTP response code is 200.
3. Getting news item by id: a GET request to the endpoint `api/newsfeed/{id}` returns the details of the news item for the {id}. If there is no news item for the {id}, status code 404 is returned. On success, status code 200 is returned.
4. Getting all news filtered by the authorName property: a GET request to the endpoint `api/newsfeed?AuthorNames={AuthorName}` returns the entire list of news for AuthorName. The HTTP response code is 200.
5. Deleting a news item by id: a DELETE request to the endpoint `api/newsfeed/{id}` deletes the corresponding news item. If there is no news item for {id}, then return status code 404. On success, return status code 204.



Change the API endpoints of the project in the following way:
- For the "Getting all news" endpoint, you need to think about performance. The first query to GET hits the database. For the second query to GET all news, you need to get the response faster using the .NET core in-memory cache mechanism. To perceive the difference between the first and second requests, the service takes care of adding a delay of 2 seconds to imitate a long query to the database.
- Important: Any operation that changes the news list should delete the in-memory cache. Tests take care of testing this.



Definition of News model:

+ id - The ID of the news item. [INTEGER]
+ title - The title of the news item. [STRING]
+ body - The content of the news item. [STRING]
+ authorName - The author name of the news item. [STRING]
+ allowComments - The flag that shows if the news item's comments are available. [BOOLEAN]
+ dateCreated - The date when the news item was created in UTC (GMT + 0). [EPOCH INTEGER]


## Example requests and responses with headers


**Request 1:**

GET request to api/newsfeed

The response code will be 200 with a list of the news feed item's details, with a time delay of 2 seconds:
```
[
{
    id: 1,
    title: "news title 1",
    authorName: "Rick D.",
    body: "Advantage old had otherwise sincerity dependent additions.",
    allowComments: true,
    dateCreated: 1573843210
} 
]
```


**Request 2:**

GET request to api/newsfeed

The response code will be 200 with a list of the news feed item's details, without a time delay of 2 seconds because the memory cache mechanism was used:
```
[
{
    id: 1,
    title: "news title 1",
    authorName: "Rick D.",
    body: "Advantage old had otherwise sincerity dependent additions.",
    allowComments: true,
    dateCreated: 1573843210
} 
]
```

**Request 3:**

DELETE request to api/newsfeed/1

The response code will be 204, and this should clean the memory cache of the news feed.
