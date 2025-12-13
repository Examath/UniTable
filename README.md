# UniTable
Visually plan your entire semester at Adelaide University

<img width="2880" height="1824" alt="UniTable 1 5" src="https://github.com/user-attachments/assets/e4391573-91fc-4a14-8235-f60a6c2d575c" />

UniTable takes class time information from the [Adelaide University](https://adelaideuni.edu.au/study/) website and presents the whole semester in a novel highly compact format. To start, simply import the courses you intend to study and make your timetable selections. UniTable visualizes the entire semester in one graphic, rather than week by week, giving you a definitive overview over all your courses. 

You can use UniTable to:
- Efficiently plan your timetable during enrolment, accounting for factors like intermittent classes and commuting that would be difficult with other applications. 
- Create calendar-like print outs of the entire semester, underlayed with course schedules, which you can use to aid in planning. No need to flick back and forth through several weeky schedules.
- Access your timetable and class location details offline. No more waiting on MyAdelaide to load. 

## How to Use
I have been using UniTable to plan my semester for the last four years. However, as it is a personal project, I have yet to make it fully user friendly. If you'd like to use it, feel free to contact me at <examathematics@gmail.com>, and I'll send you a binary and get you oriented.

- Press [+ Add New Course] to open the Course Properties & Data dialog. Enter in the URL or the course code and then press [Fetch URL] to fetch timetable data. Give the course a meaningful colour and press [Ok]. Add more courses as necessary.
- Use the drop downs in the left plane to change the selected class.
- Save.
- In Settings, adjust Scale and theme, as well as the size of the window as required.

The latest version is **v1.5**

Note only the 'Adelaide City East Campus' is currently supported because I am lazy. Please contact me if you would like to go to Mawson Lakes, or another university.

_Disclaimer: This project is not associated with Adelaide University of the former University of Adelaide in any way._

## Future Work
UniTable is an active project and over time new features will be added.

**Version 2.0** would be the production-ready version. This would improve the user experience using help guides and intuitive wizards and broaden the use cases.
Particular features that may be included are:

-	The ability to work with multiple universities.
-	Persisting multiple timetable selection combinations and switching between them.
-	Analysing the bus fares, time between classes and time in class for each timetable option.
-	Printing the timetable.
-	Exporting a timetable (.utt) that can be imported into Line/ALPS, allowing you to, for example, set deadlines of tasks to the start of lessons.

There are no plans to support automatic timetable planning as (1) automatic planning sucks, and (2) if you really wanted to put your life in the hands of an algorithm, MyAdelaide ~already~ **edit: used to** support this.

## Past Work
I developed UniTable (then **CUACV**) as a first year in January 2023, just before enrolment opened. I made it because I realized that the planning tools provided by the university were too lacking to my taste. CUACV visualized the whole semester, not just a single week.

**Version 1.2** (July 2023) added some much-requested features, including:

-	Day-of-the-month on each cell (rather than just the Sunday).
-	Better summary of class times and locations when there is variation.
-	Basic persistence of the last selected timetable when the application restarts


**Version 1.3** (Dec 2024) was a significant overhaul of the internal workings of the application. This allowed for two significant improvements:
- You can now open and save timetables to your local computer. Now you can create multiple versions of a timetable to compare and contrast, and open old timetables.
- You can directly add, edit, remove, and update courses through the user interface. Programming in .cuacv is no longer required for most tasks.


**Version 1.4** (Feb 2025) some UI and bug fixes, including:
- Adding days of the week and hourly gridlines to the timetable for readability, and the ability to resize the timetable.
- The ability to hide class type headings and markers for compactness and clarity.

**Version 1.5** (Dec 2025) supports new Adelaide University.
-	The ability to take timetable data directly the URL of the relevant course page by interpreting HTML, and the ability to refresh this to update availability. Timetable data can also be imported from a HTML file located on the disk. The ability to import from the clipboard is retained - but note that the text copied must be raw HTML. No data in the .cuacv format is required, though the ability to import legacy .cuacv documents is retained.
- Updated _Edit Course Properties & Data_ (previously _Edit Course Header_) to support direct import.
- The ability to specify the semester.
- Added lines dividing each month, as well as indicators of the hour of the day.
- Many other bug fixes, UX improvements and performance enhancements.
