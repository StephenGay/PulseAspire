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
    
        initialize: function(calendarSelector, events, resources, dotNetRef) {

            var containerEl = document.getElementById('external-events');
        new FullCalendar.Draggable(containerEl, {
            
              itemSelector: '.fc-event',
                eventData: function (eventEl) {
                    
                    return {
                        id: eventEl.id,
                        PlanId: eventEl.id,
                  title: eventEl.innerText.trim()
                }
              }
            });
        
            var calendarEl = document.querySelector(calendarSelector);
            var calendar = new FullCalendar.Calendar(calendarEl, {
                schedulerLicenseKey: 'CC-Attribution-NonCommercial-NoDerivatives',
                /* plugins: [ 'resourceTimeline', 'timeGrid', 'dayGrid', 'interaction' ],  */
                height: 'auto',
                selectable: true,
                editable: true,
                /*themeSystem: 'Superhero',*/
                droppable: true,
                nowIndicator: true,
                /*   aspectRatio: 1.8,
                  scrollTime: '00:00', */
                  headerToolbar: {
                    left: 'today prev,next',
                    center: 'title',
                      right: 'resourceTimelineDay,resourceTimelineThreeDays,timeGridWeek,dayGridMonth,listWeek'
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
                drop: function (arg) {
                    dotNetRef.invokeMethodAsync('OnEventDropped',
                        arg.draggedEl.id,
                        arg.dateStr,
                        arg.resource ? arg.resource.id : null,
                        '00:30'
                    );
                    arg.draggedEl.parentNode.removeChild(arg.draggedEl);
                },
                //drop: dropUnplannedWO(arg, dotNetRef),
                  //  console.log('drop date: ' + arg.dateStr)

                  //  if (arg.resource) {
                  //      console.log('drop resource: ' + arg.resource.id)
                  //  }

                  //    
        
                  //},
                  eventReceive: function(arg) { // called when a proper external event is dropped
                    console.log('eventReceive', arg.event);
                  },
                  eventDrop: function(info) { // called when an event (already on the calendar) is moved
                      dotNetRef.invokeMethodAsync('OnEventChanged',
                          info.event.id,
                          info.event.start,
                          info.event.end,
                          info.newResource ? info.newResource.id : 'Same'
                      );
                },
                eventResize: function (info) { // called when an event (already on the calendar) is moved
                    dotNetRef.invokeMethodAsync('OnEventResized',
                        info.event.id,
                        info.event.start,
                        info.event.end
                    );
                },
                eventClick: function (info) { // called when an event (already on the calendar) is moved
                    dotNetRef.invokeMethodAsync('OnEventClicked',
                        info.event.id
                    );
                }
            }); 
            calendar.render();
        },
        updateResources: function (resources) {
            //var calendarEl = document.getElementById('calendar');
            var calendar = document.getElementById('calendar'); // FullCalendar.getCalendar(calendarEl);
            calendar.removeAllResources();
            calendar.addResource(resources);
        }
    };
    function changeCalendarView(viewName) {
        var calendarEl = document.getElementById('calendar');
        var calendar = FullCalendar.getCalendar(calendarEl);
        calendar.changeView(viewName);
};


//function dropUnplannedWO (arg, dotNetRef) {
//    dotNetRef.invokeMethodAsync('OnEventDropped',
//        arg.id,
//        arg.dateStr,
//        arg.resource ? arg.resource.id : null,
//                        arg.duration
//    );
//}