document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll("[data-jalali-picker]").forEach(initJalaliPicker);
});

function initJalaliPicker(root) {
  const hiddenInput = root.querySelector('input[name="ReserveDateJalali"]');
  const toggle = root.querySelector("[data-jalali-toggle]");
  const timeInput = root.querySelector("[data-jalali-time]");
  const calendar = root.querySelector("[data-jalali-calendar]");

  if (!hiddenInput || !toggle || !timeInput || !calendar) {
    return;
  }

  const today = gregorianToJalali(new Date());
  const initial = parseJalaliDateTime(hiddenInput.value) || {
    year: today.year,
    month: today.month,
    day: today.day,
    time: "18:00",
  };

  const state = {
    viewYear: initial.year,
    viewMonth: initial.month,
    selectedYear: initial.year,
    selectedMonth: initial.month,
    selectedDay: initial.day,
  };

  timeInput.value = initial.time;

  const syncValue = () => {
    const date = formatJalaliDate(state.selectedYear, state.selectedMonth, state.selectedDay);
    const time = timeInput.value || "00:00";
    hiddenInput.value = `${toPersianDigits(date)} ${toPersianDigits(time)}`;
    toggle.textContent = `${toPersianDigits(date)} - ${toPersianDigits(time)}`;
  };

  const closeCalendar = () => {
    calendar.hidden = true;
  };

  const openCalendar = () => {
    renderCalendar(calendar, state, syncValue, closeCalendar);
    calendar.hidden = false;
  };

  toggle.addEventListener("click", () => {
    if (calendar.hidden) {
      openCalendar();
    } else {
      closeCalendar();
    }
  });

  timeInput.addEventListener("change", syncValue);
  timeInput.addEventListener("input", syncValue);

  document.addEventListener("click", (event) => {
    if (!root.contains(event.target)) {
      closeCalendar();
    }
  });

  syncValue();
  renderCalendar(calendar, state, syncValue, closeCalendar);
}

function renderCalendar(calendar, state, syncValue, closeCalendar) {
  const monthNames = [
    "فروردین",
    "اردیبهشت",
    "خرداد",
    "تیر",
    "مرداد",
    "شهریور",
    "مهر",
    "آبان",
    "آذر",
    "دی",
    "بهمن",
    "اسفند",
  ];
  const weekDays = ["ش", "ی", "د", "س", "چ", "پ", "ج"];
  const today = gregorianToJalali(new Date());
  const daysInMonth = getJalaliMonthLength(state.viewYear, state.viewMonth);
  const firstDay = jalaliToGregorian(state.viewYear, state.viewMonth, 1);
  const firstWeekIndex = firstDay ? (firstDay.getDay() + 1) % 7 : 0;

  calendar.innerHTML = "";

  const header = document.createElement("div");
  header.className = "jalali-calendar-header";

  const next = document.createElement("button");
  next.type = "button";
  next.className = "jalali-calendar-nav";
  next.textContent = "‹";
  next.setAttribute("aria-label", "ماه بعد");
  next.addEventListener("click", () => {
    moveMonth(state, 1);
    renderCalendar(calendar, state, syncValue, closeCalendar);
  });

  const title = document.createElement("div");
  title.className = "jalali-calendar-title";
  title.textContent = `${monthNames[state.viewMonth - 1]} ${toPersianDigits(String(state.viewYear))}`;

  const previous = document.createElement("button");
  previous.type = "button";
  previous.className = "jalali-calendar-nav";
  previous.textContent = "›";
  previous.setAttribute("aria-label", "ماه قبل");
  previous.addEventListener("click", () => {
    moveMonth(state, -1);
    renderCalendar(calendar, state, syncValue, closeCalendar);
  });

  header.append(next, title, previous);

  const grid = document.createElement("div");
  grid.className = "jalali-calendar-grid";

  weekDays.forEach((day) => {
    const item = document.createElement("div");
    item.className = "jalali-calendar-weekday";
    item.textContent = day;
    grid.appendChild(item);
  });

  for (let index = 0; index < firstWeekIndex; index += 1) {
    const empty = document.createElement("span");
    empty.className = "jalali-calendar-empty";
    grid.appendChild(empty);
  }

  for (let day = 1; day <= daysInMonth; day += 1) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "jalali-calendar-day";
    button.textContent = toPersianDigits(String(day));

    if (
      state.viewYear === state.selectedYear &&
      state.viewMonth === state.selectedMonth &&
      day === state.selectedDay
    ) {
      button.classList.add("selected");
    }

    if (
      state.viewYear === today.year &&
      state.viewMonth === today.month &&
      day === today.day
    ) {
      button.classList.add("today");
    }

    button.addEventListener("click", () => {
      state.selectedYear = state.viewYear;
      state.selectedMonth = state.viewMonth;
      state.selectedDay = day;
      syncValue();
      renderCalendar(calendar, state, syncValue, closeCalendar);
      closeCalendar();
    });

    grid.appendChild(button);
  }

  calendar.append(header, grid);
}

function moveMonth(state, offset) {
  state.viewMonth += offset;

  if (state.viewMonth > 12) {
    state.viewMonth = 1;
    state.viewYear += 1;
  }

  if (state.viewMonth < 1) {
    state.viewMonth = 12;
    state.viewYear -= 1;
  }
}

function parseJalaliDateTime(value) {
  const normalized = normalizeDigits(value || "").replaceAll("-", "/").replaceAll(".", "/").trim();
  const match = normalized.match(/^(\d{4})\/(\d{1,2})\/(\d{1,2})(?:\s+(\d{1,2}):(\d{1,2}))?/);

  if (!match) {
    return null;
  }

  const year = Number(match[1]);
  const month = Number(match[2]);
  const day = Number(match[3]);
  const hour = match[4] ? match[4].padStart(2, "0") : "00";
  const minute = match[5] ? match[5].padStart(2, "0") : "00";

  if (month < 1 || month > 12 || day < 1 || day > getJalaliMonthLength(year, month)) {
    return null;
  }

  return {
    year,
    month,
    day,
    time: `${hour}:${minute}`,
  };
}

function formatJalaliDate(year, month, day) {
  return `${year}/${String(month).padStart(2, "0")}/${String(day).padStart(2, "0")}`;
}

function getJalaliMonthLength(year, month) {
  if (month <= 6) {
    return 31;
  }

  if (month <= 11) {
    return 30;
  }

  return jalaliToGregorian(year, 12, 30) ? 30 : 29;
}

function jalaliToGregorian(year, month, day) {
  const key = `${year}/${month}/${day}`;
  jalaliToGregorian.cache ||= new Map();

  if (jalaliToGregorian.cache.has(key)) {
    return jalaliToGregorian.cache.get(key);
  }

  const center = Date.UTC(year + 621, month - 1, day);

  for (let offset = -370; offset <= 370; offset += 1) {
    const candidate = new Date(center + offset * 86400000);
    const jalali = gregorianToJalali(candidate);

    if (jalali.year === year && jalali.month === month && jalali.day === day) {
      jalaliToGregorian.cache.set(key, candidate);
      return candidate;
    }
  }

  jalaliToGregorian.cache.set(key, null);
  return null;
}

function gregorianToJalali(date) {
  const formatter = new Intl.DateTimeFormat("en-US-u-ca-persian", {
    year: "numeric",
    month: "numeric",
    day: "numeric",
  });
  const parts = Object.fromEntries(formatter.formatToParts(date).map((part) => [part.type, part.value]));

  return {
    year: Number(parts.year),
    month: Number(parts.month),
    day: Number(parts.day),
  };
}

function normalizeDigits(value) {
  return value.replace(/[۰-۹٠-٩]/g, (digit) => {
    const persian = "۰۱۲۳۴۵۶۷۸۹".indexOf(digit);
    if (persian >= 0) {
      return String(persian);
    }

    return String("٠١٢٣٤٥٦٧٨٩".indexOf(digit));
  });
}

function toPersianDigits(value) {
  return value.replace(/\d/g, (digit) => "۰۱۲۳۴۵۶۷۸۹"[Number(digit)]);
}
