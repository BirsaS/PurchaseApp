(() => {
    const picker = document.getElementById('delivery-date-picker');
    const dateField = document.getElementById('delivery-date-display');
    const dateValue = document.getElementById('DesiredDeliveryDate');
    const calendar = document.getElementById('delivery-date-calendar');
    const toggle = document.getElementById('delivery-date-toggle');
    const monthLabel = document.getElementById('delivery-date-month');
    const daysGrid = document.getElementById('delivery-date-days');
    const monthNames = ['januar', 'februar', 'marec', 'april', 'maj', 'junij', 'julij', 'avgust', 'september', 'oktober', 'november', 'december'];
    const weekdays = ['pon', 'tor', 'sre', 'čet', 'pet', 'sob', 'ned'];
    let visibleMonth = new Date();
    visibleMonth.setDate(1);
    
    function parseDisplayedDate(value) {
        const match = value.trim().match(/^(\d{1,2})\.\s*(\d{1,2})\.\s*(\d{4})$/);
        if (!match) return null;
    
        const day = Number(match[1]);
        const month = Number(match[2]);
        const year = Number(match[3]);
        const date = new Date(year, month - 1, day);
        return date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day ? date : null;
    }
    
    function setDate(date) {
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        dateField.value = `${day}. ${month}. ${year}`;
        dateValue.value = `${year}-${month}-${day}`;
        dateField.setCustomValidity('');
        calendar.hidden = true;
        toggle.setAttribute('aria-expanded', 'false');
    }
    
    function renderCalendar() {
        monthLabel.textContent = `${monthNames[visibleMonth.getMonth()]} ${visibleMonth.getFullYear()}`;
        daysGrid.replaceChildren();
    
        weekdays.forEach(day => {
            const heading = document.createElement('span');
            heading.className = 'date-picker-weekday';
            heading.textContent = day;
            daysGrid.appendChild(heading);
        });
    
        const offset = (visibleMonth.getDay() + 6) % 7;
        for (let index = 0; index < offset; index++) {
            daysGrid.appendChild(document.createElement('span'));
        }
    
        const daysInMonth = new Date(visibleMonth.getFullYear(), visibleMonth.getMonth() + 1, 0).getDate();
        for (let day = 1; day <= daysInMonth; day++) {
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'btn btn-sm btn-light';
            button.textContent = day;
            button.setAttribute('aria-label', `${day}. ${monthNames[visibleMonth.getMonth()]} ${visibleMonth.getFullYear()}`);
            button.addEventListener('click', () => setDate(new Date(visibleMonth.getFullYear(), visibleMonth.getMonth(), day)));
            daysGrid.appendChild(button);
        }
    }
    
    const initialDate = dateValue.value.match(/^(\d{4})-(\d{2})-(\d{2})$/);
    if (initialDate) {
        const date = new Date(Number(initialDate[1]), Number(initialDate[2]) - 1, Number(initialDate[3]));
        dateField.value = `${String(date.getDate()).padStart(2, '0')}. ${String(date.getMonth() + 1).padStart(2, '0')}. ${date.getFullYear()}`;
        visibleMonth = new Date(date.getFullYear(), date.getMonth(), 1);
    }
    
    toggle.addEventListener('click', () => {
        calendar.hidden = !calendar.hidden;
        toggle.setAttribute('aria-expanded', String(!calendar.hidden));
        if (!calendar.hidden) renderCalendar();
    });
    
    document.getElementById('delivery-date-previous').addEventListener('click', () => {
        visibleMonth.setMonth(visibleMonth.getMonth() - 1);
        renderCalendar();
    });
    
    document.getElementById('delivery-date-next').addEventListener('click', () => {
        visibleMonth.setMonth(visibleMonth.getMonth() + 1);
        renderCalendar();
    });
    
    dateField.addEventListener('input', () => {
        const date = parseDisplayedDate(dateField.value);
        dateValue.value = date ? `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}` : '';
        dateField.setCustomValidity(dateField.value && !date ? 'Vnesite veljaven datum v obliki dd. mm. yyyy.' : '');
    });
    
    picker.closest('form').addEventListener('submit', event => {
        const date = parseDisplayedDate(dateField.value);
        if (!date) {
            dateField.setCustomValidity('Vnesite veljaven datum v obliki dd. mm. yyyy.');
            dateField.reportValidity();
            event.preventDefault();
            return;
        }
    
        dateValue.value = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
    });
    
    picker.addEventListener('keydown', event => {
        if (event.key === 'Escape') {
            calendar.hidden = true;
            toggle.setAttribute('aria-expanded', 'false');
        }
    });
    
    renderCalendar();
})();
