//import dayGridPlugin from '@fullcalendar/daygrid'

/*window.fullCalendarInterop = {
    
    initialize: function (calendarSelector, events) {
        
        var calendarEl = document.querySelector(calendarSelector);
        var calendar = new FullCalendar.Calendar(calendarEl, {

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
}*/

window.fullCalendarInterop = {
    
        initialize: function(calendarSelector, events, resources) {

            var containerEl = document.getElementById('external-events');
            new FullCalendar.Draggable(containerEl, {
              itemSelector: '.fc-event',
              eventData: function(eventEl) {
                return {
                  title: eventEl.innerText.trim()
                }
              }
            });
        
            var calendarEl = document.querySelector(calendarSelector);
            var calendar = new FullCalendar.Calendar(calendarEl, {

                /* plugins: [ 'resourceTimeline', 'timeGrid', 'dayGrid', 'interaction' ],  */
                selectable: true,
                editable: true,
                droppable: true,
                /* nowIndicator: true,
                  aspectRatio: 1.8,
                  scrollTime: '00:00', */
                  headerToolbar: {
                    left: 'today prev,next',
                    center: 'title',
                    right: 'resourceTimelineDay,resourceTimelineThreeDays,timeGridWeek,dayGridMonth'
                  },
                  initialView: 'resourceTimelineDay',
                  views: {
                    resourceTimelineThreeDays: {
                      type: 'resourceTimeline',
                      duration: { days: 3 },
                      buttonText: '3 days'
                    }
                  },
                  resourceAreaWidth: '20%',
                  resourceAreaColumns: [
                    {
                      headerContent: 'Equipment',
                      field: 'title'
                    }
                  ],
                  resources: resources,
                events: events,
                drop: function(arg) {
                    console.log('drop date: ' + arg.dateStr)

                    if (arg.resource) {
                      console.log('drop resource: ' + arg.resource.id)
                    }

                    // is the "remove after drop" checkbox checked?
        
                      arg.draggedEl.parentNode.removeChild(arg.draggedEl);
        
                  },
                  eventReceive: function(arg) { // called when a proper external event is dropped
                    console.log('eventReceive', arg.event);
                  },
                  eventDrop: function(arg) { // called when an event (already on the calendar) is moved
                    console.log('eventDrop', arg.event);
                  }
            }); 
            calendar.render();
        }
    };
    function changeCalendarView(viewName) {
        var calendarEl = document.getElementById('calendar');
        var calendar = FullCalendar.getCalendar(calendarEl);
        calendar.changeView(viewName);
    } 