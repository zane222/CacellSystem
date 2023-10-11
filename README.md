# CacellSystem
CacellSystem is a website made with Unity to help manage a computer and/or cellphone repair store. CacellSystem is an extension of RepairShopr and requires RepairShopr's REST API for use as a database. CacellSystem can read, edit, etc. all tickets and customers that were previouly made by using RepairShopr's website and both website's tickets and customers are synced to the other and are viewable and edit-able by the other. CacellSystem is meant to be a replacement for using RepairShopr's website, while still being able to use RepairShopr's website for things you may use every once in a while if it has a feature you sometimes use.

I am CacellSystem's soul developer and it took me about a year of development to develop it to this state (basicly no bugs and it's not missing any important features).

I work at a computer and cellphone repair store and CacellSystem is currently being used at that store. I created it becuase I have a lot of issues with RepairShopr, and wanted a better system.

## Pros and Cons of CacellSystem vs RepairShopr's website
### Pros
* Has faster load times
* You can basically all of the program without touching the mouse, just the keyboard
* The image of the ticket label comes up faster and with less clicks and is the primary way of viewing a ticket
* It tells you all the previously used passwords of a customer without you having to look through each ticket (incase we forgot to ask for the password, we can just look at this)
* No dropdowns (I don't like dropdowns because they require you to click, then find the correct option, then click again, and sometimes they cover up things, and also sometimes they are off screen)
* More efficient with how screen space is used (not having stuff we don't use that we have to scroll past and click past to get to the stuff we use)
* Can make a ticket in a way that is very consistent and neat with less effort than manually making it look neat (I'm picky and I like it like that much more)
* You can choose the primary phone number for a customer without having to fiddle around with it until it's correct
* Makes it easier to notice if you're printing to the wrong printer

### Cons
Basicly all these things are not used more than once a month at the store I work at, when we need one of these features, we just use RepairShopr's website
* Can't You can merge customers
* Can't add attachments to tickets and view attachments added to tickets
* Can't make, view, or print invoices or estimates
* Can't send SMS
* Can't make a rework of a ticket
* Can't clock in and out (we just have RepairShopr's website on our phone to clock in and out, then have CacellSystem on the computers)
* Can't manage or view clock ins and outs of employees

## How to use it for your store
I don't have an automatic way of you using the website, you would have to change multiple things in the code, it would be difficult. You can just email me (zamo2022@gmail.com) and I'll can help you

### If you want to do it manually
* Get an [api key](https://feedback.repairshopr.com/knowledgebase/articles/376312-repairshopr-rest-api-build-custom-extensions-app) from RepairShopr
* Change API_KEY in SignInManager.cs to your api key (this is only for if you want to develop the project, if you don't want to develop it, you don't have to do this)
* Change DOMAIN_NAME in Main.cs to the text in the leftmost part of the url at the top of your browser when your on RepairShopr's website (only the text before the first dot)
* Create a [firebase realtime database](https://console.firebase.google.com/) (it's free) that looks exactly like this:

  ![Screenshot 2023-10-10 190531](https://github.com/zane222/CacellSystem/assets/51272566/1c465bf3-34c4-4e33-b573-be3aeb91ad33)
* Put the link to your firebase database in place of mine in SignInManager.cs (lines 29 and 43, only change the stuff before the .com)
* Build the project to webGL
* Host the website somewhere (I use [firebase hosting](https://firebase.google.com/docs/cli), it works well and is free)

It may still not work, this is probably because the ticket types for your RepairShopr aren't default (or the RepairShopr I developed it on aren't default and yours are), this would be super difficult to fix, you can email me if this happens and I can help you.
