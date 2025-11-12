//import dayGridPlugin from '@fullcalendar/daygrid'

window.fullCalendarInterop = {
    
    initialize: function (calendarSelector, events) {
        
        var calendarEl = document.querySelector(calendarSelector);
        var calendar = new FullCalendar.Calendar(calendarEl, {
            //plugins: [dayGridPlugin],
            initialView: 'dayGridMonth',
            selectable: true,
            editable: true,
            headerToolbar: {
                left: 'prev,next',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
            },
            views: {
                dayGridMonth: { buttonText: 'Month' },
                timeGridWeek: { buttonText: 'Week' },
                timeGridDay: { buttonText: 'Day' },
                listWeek: { buttonText: 'Agenda' }
            },
            events: events
        });
        calendar.render();
    }
};
function changeCalendarView(viewName) {
    var calendarEl = document.getElementById('calendar');
    var calendar = FullCalendar.getCalendar(calendarEl);
    calendar.changeView(viewName);
}