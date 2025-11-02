
# Web Developer Technical Test-B

This project was a technical test to foundever to show technical skills as a Web Developer, where the goal was to implement a feature of favorite pokemons with certain restrictions to the user. below you can find the instructions to run locally.

## Technical and Design decisions

I wanted to make an approach to the KISS principle, thats why I keep contained most of the logic in the home controller, keeping things centralized.

Also I saw I didn't needed to save all the pokemons data in the database but have a reference between them, and that's when I thought why don't just make a table with a reference to the user id and the pokemon id, and thats where enters the favorites table

When I was implementing the poke api service, I remember that I used interfaces to make the main methods in case for example you change from AWS to Azure don't need to redesign architecture everything just change the implementation 

Also, I wanted to implement the loading screens but I don't remember the way to do it.
## Installation

Clone the fork of the repository, I did this because I can't create a branch or push any change in the main repository.


```bash
  git clone git@github.com:Zuppremo/DeveloperTest2.git
  git checkout deiby_montenegro
  git pull 
```

This will give all the needed source code

Now we need to make the changes to the database, after executing the provided creating of the database with the table users we need to add another table for the feature

```
CREATE TABLE favorites (
    favorite_id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    pokemon_id int,
    user_id int,
    FOREIGN KEY (user_id) REFERENCES dbo.users(user_id)
);
```
Based on the name you give to your database you will need to change the initial catalog value in your connection string in ```Web.config``` file on the solution

```
<connectionStrings>
  <add name="Context" connectionString="metadata=res://*/Models.DataModel.csdl|res://*/Models.DataModel.ssdl|res://*/Models.DataModel.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=(localdb)\MSSQLLocalDB;initial catalog="{YourDatabaseName};trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework&quot;" 
 providerName="System.Data.EntityClient" />
</connectionStrings>
```
Also sometimes inside the method:
```
protected override void OnModelCreating(DbModelBuilder modelBuilder)
``` 
changes to a throw
this can broke the connection, what I did was change that automatic throw to: 
```
modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
```
Keep in mind that the connection can be seen in the Visual Studio Server Explorer ensure that you are connected to the database here!

![App Screenshot](https://imgur.com/LlylV9v)


After that you should be able to create an user in the database, and after login it will show you all the pokemons as expected



    
## Demo

https://youtu.be/HrNgNon6Qxo
## Documentation

Database Entity Diagram

![ERD Diagram](https://imgur.com/clA8i5V)


## Screenshots

![Pokemon List](https://imgur.com/49bhjMU)
![No favorites](https://imgur.com/nsgk1sx)
![Favorites List](https://imgur.com/SMGW7Z7)
![Can't add favorite](https://imgur.com/80t07Qd)
![Find Pokemon](https://imgur.com/BH4dDrC)


## Authors

- [@zuppremo](https://www.github.com/zuppremo)


## Feedback

If you have any feedback, please reach out to us at deibymontenegrob@gmail.com

