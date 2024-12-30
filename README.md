# UniTable
Visually plan your entire semester timetable at the University of Adelaide

![image](https://github.com/user-attachments/assets/2e77953b-92f5-4e41-8535-298d9759c88f)



UniTable takes class time information from the [UofA Course Planner](https://access.adelaide.edu.au/courses/) website, and presents the whole semester in a novel highly compact format. To start, simply copy-paste the timetables for the courses you want to enroll in. UniTable visualizes the entire semester in one graphic, rather than week by week. This makes it way easier to enroll in courses with intermittent classes (e.g. practicals) and choose the most optimal timetable. 

UniTable can be used to create a single, highly-compact timetable that's relevant for the entire year. No more needing to log in to MyAdelaide every week. 

I have been using UniTable to plan my semester for the last three years. However, as it is a personal project, it does have a bit of a learning curve. If you'd also like to use it, feel free to contact me at <examathematics@gmail.com>, and I'll get you started.

The latest version is **v1.3**

_Disclaimer: This project is not associated with the University of Adelaide in any way._
## Future Work
UniTable is an active project and over time new features will be added.

**Version 1.4** (Dec 2025) will support the new Adelaide University.

**Version 2.0** would be the production-ready version. This would improve the user experience using help guides and intuitive wizards and broaden the use cases.
Particular features that may be included are:

-	The ability to take timetable data directly the URL of the relevant Access Adelaide page (or whatever the new university uses), and the ability to automatically refresh to update availability.
-	The ability to work with multiple universities.
-	Persisting multiple timetable selection combinations, and switching between them.
-	Analysing the bus fares, time between classes and time in class for each timetable option.
-	Printing the timetable.
-	Exporting a timetable (.utt) that can be imported into Line/ALPS, allowing you to, for example, set deadlines of tasks to the start of lessons.

There are no plans to support automatic timetable planning as (1) automatic planning sucks, and (2) if you really wanted to put your life in the hands of an algorithm, MyAdelaide already supports this.
## Past Work
I developed UniTable (then **CUACV**) as a first year in January 2023, just before enrollment opened. I made it cause I realized that the planning tools provided by the uni are too lacking to my taste. CUACV visualizes the whole semester, not just a single week.

**Version 1.2** (July 2023) added some much-requested features, including:

-	Day-of-the-month on each cell (rather than just the Sunday).
-	Better summary of class times and locations when there is variation.
-	Basic persistence of the last selected timetable when the application restarts


**Version 1.3** (Dec 2024) was a significant overhaul of the internal workings of the application. This allowed for two significant improvements:
- You can now open and save timetables to your local computer. Now you can create multiple versions of a timetable to compare and contrast, and open old timetables.
- You can directly add, edit, remove and update courses through the user interface. Programming in .cuacv is no longer required
