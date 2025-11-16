export function initCalendar(elementId, dotNetRef, initialEvents) {
    var calendarEl = document.getElementById(elementId);
    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        editable: true,
        events: initialEvents || [],  // Use provided events
        eventDrop: function(info) {
            dotNetRef.invokeMethodAsync('HandleEventDrop', 
                info.event.id, 
                info.event.start.toISOString(), 
                info.event.end ? info.event.end.toISOString() : null
            );
        }
    });
    calendar.render();
    return calendar;
} 