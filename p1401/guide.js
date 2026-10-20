// One step at a time with Back and Next, like the Probe app. "Show all the steps" puts every step on one page.
// Also provides a Material Design 3 (M3) Theme & Appearance Settings popover defaulting to Material Blue.
function each(list, fn) {
  for (var i = 0; i < list.length; i++) fn(list[i], i);
}

(function initM3Theme() {
  var lang = (document.documentElement && document.documentElement.getAttribute) ? (document.documentElement.getAttribute("lang") || "") : "";
  var isZh = lang.indexOf("zh") === 0;
  var L = isZh ? {
    settings: "设置",
    title: "主题与外观 (Material Design 3)",
    palette: "配色方案：",
    mode: "外观模式：",
    dark: "深色模式",
    light: "浅色模式",
    blue: "蓝色 (默认)",
    teal: "青色",
    green: "绿色",
    purple: "紫色",
    orange: "橙色",
    rose: "玫红",
    custom: "自定义颜色"
  } : {
    settings: "Settings",
    title: "Theme & Appearance (Material 3)",
    palette: "Color palette:",
    mode: "Mode:",
    dark: "Dark mode",
    light: "Light mode",
    blue: "Blue (Default)",
    teal: "Teal",
    green: "Green",
    purple: "Purple",
    orange: "Orange",
    rose: "Rose",
    custom: "Custom"
  };

  var presets = [
    { id: "blue", label: L.blue, hex: "#1b6ef3" },
    { id: "teal", label: L.teal, hex: "#00838f" },
    { id: "green", label: L.green, hex: "#1e8e3e" },
    { id: "purple", label: L.purple, hex: "#7c4dff" },
    { id: "orange", label: L.orange, hex: "#e65100" },
    { id: "rose", label: L.rose, hex: "#d81b60" }
  ];

  var state = { scheme: "blue", dark: true, custom: "#1b6ef3" };
  var validSchemes = { blue: 1, teal: 1, green: 1, purple: 1, orange: 1, rose: 1, custom: 1 };
  try {
    var raw = window.localStorage && window.localStorage.getItem("1401_m3_theme");
    if (raw) {
      var parsed = JSON.parse(raw);
      if (parsed && parsed.scheme && validSchemes[parsed.scheme]) state.scheme = parsed.scheme;
      if (parsed && typeof parsed.dark === "boolean") state.dark = parsed.dark;
      if (parsed && typeof parsed.custom === "string" && /^#[0-9a-fA-F]{6}$/.test(parsed.custom)) state.custom = parsed.custom;
    }
  } catch (e) {}

  function hexToHsl(hex) {
    var clean = (hex || "#1b6ef3").replace("#", "");
    var n = parseInt(clean, 16);
    if (isNaN(n)) n = 0x1b6ef3;
    var r = ((n >> 16) & 255) / 255, g = ((n >> 8) & 255) / 255, b = (n & 255) / 255;
    var max = Math.max(r, g, b), min = Math.min(r, g, b), h = 215, s = 0, l = (max + min) / 2;
    if (max !== min) {
      var d = max - min;
      s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
      if (max === r) h = (g - b) / d + (g < b ? 6 : 0);
      else if (max === g) h = (b - r) / d + 2;
      else h = (r - g) / d + 4;
      h *= 60;
    }
    return { h: h, s: s, l: l };
  }

  function hslToHex(h, s, l) {
    h = ((h % 360) + 360) % 360;
    s = Math.max(0, Math.min(1, s));
    l = Math.max(0, Math.min(1, l));
    var c = (1 - Math.abs(2 * l - 1)) * s;
    var x = c * (1 - Math.abs(((h / 60) % 2) - 1));
    var m = l - c / 2, r = 0, g = 0, b = 0;
    if (h < 60) { r = c; g = x; }
    else if (h < 120) { r = x; g = c; }
    else if (h < 180) { g = c; b = x; }
    else if (h < 240) { g = x; b = c; }
    else if (h < 300) { r = x; b = c; }
    else { r = c; b = x; }
    function toHex(v) {
      var s2 = Math.max(0, Math.min(255, Math.round((v + m) * 255))).toString(16);
      return s2.length < 2 ? "0" + s2 : s2;
    }
    return "#" + toHex(r) + toHex(g) + toHex(b);
  }

  function computeTokens() {
    if (state.scheme === "blue") {
      if (state.dark) {
        return {
          surface: "#0f141c",
          surfaceLow: "#171c26",
          surfaceContainer: "#1b202b",
          surfaceHigh: "#252b37",
          outlineVariant: "#414855",
          outline: "#8b92a0",
          primary: "#a8c7fa",
          onPrimary: "#062e6f",
          primaryContainer: "#0842a0",
          onPrimaryContainer: "#d3e3fd",
          secondary: "#7fcfff",
          onSurface: "#e2e7f0",
          onSurfaceVariant: "#b6bfd0",
          error: "#f28b82",
          warning: "#fdd663",
          button: "#1b6ef3",
          onButton: "#ffffff"
        };
      }
      return {
        surface: "#f8f9ff",
        surfaceLow: "#f0f4fa",
        surfaceContainer: "#eaeef6",
        surfaceHigh: "#e2e7f0",
        outlineVariant: "#c4c6d0",
        outline: "#74777f",
        primary: "#0b57d0",
        onPrimary: "#ffffff",
        primaryContainer: "#d3e3fd",
        onPrimaryContainer: "#041e49",
        secondary: "#00639b",
        onSurface: "#191c20",
        onSurfaceVariant: "#44474e",
        error: "#b3261e",
        warning: "#b06000",
        button: "#0b57d0",
        onButton: "#ffffff"
      };
    }
    var seed = state.custom || "#1b6ef3";
    for (var i = 0; i < presets.length; i++) {
      if (presets[i].id === state.scheme) { seed = presets[i].hex; break; }
    }
    var hsl = hexToHsl(seed);
    var h = hsl.h, ch = Math.max(0.48, Math.min(0.92, hsl.s)), ns = Math.min(0.18, ch * 0.22);
    if (state.dark) {
      return {
        surface: hslToHex(h, ns, 0.08),
        surfaceLow: hslToHex(h, ns, 0.115),
        surfaceContainer: hslToHex(h, ns, 0.14),
        surfaceContainerHigh: hslToHex(h, ns, 0.185),
        surfaceHigh: hslToHex(h, ns, 0.185),
        outlineVariant: hslToHex(h, ns * 1.1, 0.28),
        outline: hslToHex(h, ns, 0.56),
        primary: hslToHex(h, Math.max(0.76, ch), 0.78),
        onPrimary: hslToHex(h, Math.max(0.70, ch), 0.16),
        primaryContainer: hslToHex(h, Math.max(0.68, ch), 0.30),
        onPrimaryContainer: hslToHex(h, Math.max(0.80, ch), 0.90),
        secondary: hslToHex((h + 18) % 360, Math.max(0.65, ch * 0.85), 0.74),
        onSurface: hslToHex(h, ns * 0.5, 0.91),
        onSurfaceVariant: hslToHex(h, ns * 0.7, 0.74),
        error: "#f28b82",
        warning: "#fdd663",
        button: hslToHex(h, Math.max(0.72, ch), 0.48),
        onButton: "#ffffff"
      };
    }
    return {
      surface: hslToHex(h, ns * 0.6, 0.98),
      surfaceLow: hslToHex(h, ns * 0.75, 0.955),
      surfaceContainer: hslToHex(h, ns * 0.85, 0.93),
      surfaceHigh: hslToHex(h, ns * 0.9, 0.895),
      outlineVariant: hslToHex(h, ns * 0.8, 0.78),
      outline: hslToHex(h, ns * 0.8, 0.46),
      primary: hslToHex(h, Math.max(0.72, ch), 0.40),
      onPrimary: "#ffffff",
      primaryContainer: hslToHex(h, Math.max(0.75, ch), 0.90),
      onPrimaryContainer: hslToHex(h, Math.max(0.75, ch), 0.14),
      secondary: hslToHex((h + 18) % 360, Math.max(0.65, ch * 0.85), 0.36),
      onSurface: hslToHex(h, ns, 0.12),
      onSurfaceVariant: hslToHex(h, ns, 0.30),
      error: "#b3261e",
      warning: "#b06000",
      button: hslToHex(h, Math.max(0.72, ch), 0.40),
      onButton: "#ffffff"
    };
  }

  function applyTheme() {
    var t = computeTokens();
    var root = document.documentElement;
    if (root && root.style && root.style.setProperty) {
      root.style.setProperty("--md-sys-color-surface", t.surface);
      root.style.setProperty("--md-sys-color-surface-container-low", t.surfaceLow);
      root.style.setProperty("--md-sys-color-surface-container", t.surfaceContainer);
      root.style.setProperty("--md-sys-color-surface-container-high", t.surfaceHigh);
      root.style.setProperty("--md-sys-color-outline-variant", t.outlineVariant);
      root.style.setProperty("--md-sys-color-outline", t.outline);
      root.style.setProperty("--md-sys-color-primary", t.primary);
      root.style.setProperty("--md-sys-color-on-primary", t.onPrimary);
      root.style.setProperty("--md-sys-color-primary-container", t.primaryContainer);
      root.style.setProperty("--md-sys-color-on-primary-container", t.onPrimaryContainer);
      root.style.setProperty("--md-sys-color-secondary", t.secondary);
      root.style.setProperty("--md-sys-color-on-surface", t.onSurface);
      root.style.setProperty("--md-sys-color-on-surface-variant", t.onSurfaceVariant);
      root.style.setProperty("--md-sys-color-error", t.error);
      root.style.setProperty("--md-sys-color-warning", t.warning);
      root.style.setProperty("--md-sys-color-button", t.button);
      root.style.setProperty("--md-sys-color-on-button", t.onButton);
    }
    var dyn = document.getElementById("m3-dyn-style");
    if (!dyn) {
      dyn = document.createElement("style");
      dyn.id = "m3-dyn-style";
      if (document.head && document.head.appendChild) document.head.appendChild(dyn);
    }
    dyn.textContent =
      "body{background-color:" + t.surface + ";color:" + t.onSurface + ";}" +
      ".top,.side,.pg{background:" + t.surfaceLow + ";border-color:" + t.outlineVariant + ";}" +
      ".pc,.printout,.m3-theme-panel,.bd,.counter b{background:" + t.surfaceContainer + ";border-color:" + t.outlineVariant + ";}" +
      ".pc th,.m3-theme-btn,.m3-swatch-btn,.all,.b.back,code{background:" + t.surfaceHigh + ";color:" + t.onSurface + ";border-color:" + t.outlineVariant + ";}" +
      ".nm,.pc caption,.stepno,.saw,a,a:visited,.all{color:" + t.primary + ";}" +
      ".ticker,.nav[aria-current],kbd,.m3-swatch-btn[aria-pressed='true'],.bd4{background:" + t.primaryContainer + ";color:" + t.onPrimaryContainer + ";border-color:" + t.primary + ";}" +
      ".b{background:" + t.button + ";color:" + t.onButton + ";}" +
      ".want,.ok,code,.counter b{color:" + t.secondary + ";}" +
      ".sy,.ig,.hello,.sh,.nav,.where,.foot,.opt,.miss,.ph{color:" + t.onSurfaceVariant + ";}";
    try {
      if (window.localStorage) window.localStorage.setItem("1401_m3_theme", JSON.stringify(state));
    } catch (e) {}
  }

  var syncCallbacks = [];
  applyTheme();

  window.setM3Theme = function (scheme, dark, custom) {
    if (scheme && validSchemes[scheme]) state.scheme = scheme;
    if (typeof dark === "boolean") state.dark = dark;
    else if (dark === "true" || dark === "false") state.dark = (dark === "true");
    if (typeof custom === "string" && /^#[0-9a-fA-F]{6}$/.test(custom)) state.custom = custom;
    applyTheme();
    each(syncCallbacks, function (fn) { fn(); });
  };

  window.getM3Theme = function () {
    return JSON.stringify(state);
  };

  each(document.querySelectorAll(".top"), function (top) {
    var btn = document.createElement("button");
    btn.type = "button";
    btn.className = "m3-theme-btn";
    btn.textContent = L.settings;
    btn.setAttribute("aria-expanded", "false");

    var panel = document.createElement("div");
    panel.className = "m3-theme-panel";
    panel.hidden = true;
    panel.setAttribute("hidden", "");

    var title = document.createElement("div");
    title.style.fontWeight = "600";
    title.style.marginBottom = "6px";
    title.textContent = L.title;
    panel.appendChild(title);

    var row1 = document.createElement("div");
    row1.className = "m3-theme-row";
    var lbl1 = document.createElement("span");
    lbl1.style.fontSize = "12px";
    lbl1.textContent = L.palette;
    row1.appendChild(lbl1);

    var swatchBtns = [];
    each(presets, function (p) {
      var sb = document.createElement("button");
      sb.type = "button";
      sb.className = "m3-swatch-btn";
      sb.setAttribute("data-scheme", p.id);
      var dot = document.createElement("span");
      dot.className = "m3-dot";
      dot.style.backgroundColor = p.hex;
      sb.appendChild(dot);
      sb.appendChild(document.createTextNode(p.label));
      sb.addEventListener("click", function () {
        state.scheme = p.id;
        applyTheme();
        syncUI();
      });
      swatchBtns.push(sb);
      row1.appendChild(sb);
    });

    var customWrap = document.createElement("label");
    customWrap.className = "m3-swatch-btn";
    customWrap.style.cursor = "pointer";
    var colorInput = document.createElement("input");
    colorInput.type = "color";
    colorInput.value = state.custom;
    colorInput.style.width = "16px";
    colorInput.style.height = "16px";
    colorInput.style.border = "0";
    colorInput.style.padding = "0";
    colorInput.style.background = "none";
    function onCustomColor() {
      if (/^#[0-9a-fA-F]{6}$/.test(colorInput.value)) {
        state.custom = colorInput.value;
        state.scheme = "custom";
        applyTheme();
        syncUI();
      }
    }
    colorInput.addEventListener("input", onCustomColor);
    colorInput.addEventListener("change", onCustomColor);
    customWrap.appendChild(colorInput);
    customWrap.appendChild(document.createTextNode(L.custom));
    row1.appendChild(customWrap);
    panel.appendChild(row1);

    var row2 = document.createElement("div");
    row2.className = "m3-theme-row";
    var lbl2 = document.createElement("span");
    lbl2.style.fontSize = "12px";
    lbl2.textContent = L.mode;
    row2.appendChild(lbl2);

    var darkBtn = document.createElement("button");
    darkBtn.type = "button";
    darkBtn.className = "m3-swatch-btn";
    darkBtn.textContent = L.dark;
    darkBtn.addEventListener("click", function () {
      state.dark = true;
      applyTheme();
      syncUI();
    });

    var lightBtn = document.createElement("button");
    lightBtn.type = "button";
    lightBtn.className = "m3-swatch-btn";
    lightBtn.textContent = L.light;
    lightBtn.addEventListener("click", function () {
      state.dark = false;
      applyTheme();
      syncUI();
    });

    row2.appendChild(darkBtn);
    row2.appendChild(lightBtn);
    panel.appendChild(row2);

    function syncUI() {
      each(swatchBtns, function (sb) {
        sb.setAttribute("aria-pressed", sb.getAttribute("data-scheme") === state.scheme ? "true" : "false");
      });
      colorInput.value = state.custom;
      customWrap.setAttribute("aria-pressed", state.scheme === "custom" ? "true" : "false");
      darkBtn.setAttribute("aria-pressed", state.dark ? "true" : "false");
      lightBtn.setAttribute("aria-pressed", !state.dark ? "true" : "false");
    }
    syncCallbacks.push(syncUI);
    syncUI();

    btn.addEventListener("click", function () {
      var open = panel.hidden;
      panel.hidden = !open;
      if (open) panel.removeAttribute("hidden"); else panel.setAttribute("hidden", "");
      panel.style.display = open ? "block" : "none";
      btn.setAttribute("aria-expanded", open ? "true" : "false");
    });

    top.appendChild(btn);
    top.appendChild(panel);
  });
})();

each(document.querySelectorAll(".app"), function (app) {
  var pages = app.querySelectorAll(".pg"), navs = app.querySelectorAll(".nav");
  var back = app.querySelector(".back"), next = app.querySelector(".next"), all = app.querySelector(".all");
  var status = app.querySelector(".where");
  var stepTmpl = app.getAttribute("data-step") || "Step {0} of {1}";
  var allTmpl = app.getAttribute("data-all") || "All {0} steps";
  var oneTxt = app.getAttribute("data-one") || "Show one step at a time";
  var everyTxt = app.getAttribute("data-every") || "Show all the steps on one page";
  var cur = 0, every = false;

  function show(i, focus) {
    cur = Math.max(0, Math.min(pages.length - 1, i));
    each(pages, function (p, j) {
      var hide = !every && j !== cur;
      p.hidden = hide;
      if (hide) p.setAttribute("hidden", ""); else p.removeAttribute("hidden");
      p.style.display = hide ? "none" : "";
    });
    each(navs, function (n, j) {
      if (j === cur && !every) n.setAttribute("aria-current", "step"); else n.removeAttribute("aria-current");
    });
    back.disabled = every || cur === 0;
    next.disabled = every || cur === pages.length - 1;
    all.setAttribute("aria-pressed", every ? "true" : "false");
    status.textContent = every
      ? allTmpl.replace("{0}", pages.length)
      : stepTmpl.replace("{0}", cur + 1).replace("{1}", pages.length);
    all.textContent = every ? oneTxt : everyTxt;
    if (focus) {
      var h = pages[cur].querySelector("h2");
      try { h.focus({ preventScroll: true }); } catch (e) { try { h.focus(); } catch (e2) {} }
      if (app.getBoundingClientRect().top < 0) {
        try { pages[cur].scrollIntoView({ block: "start" }); } catch (e) { pages[cur].scrollIntoView(true); }
      }
    }
  }

  each(navs, function (n, j) {
    n.addEventListener("click", function (e) {
      e.preventDefault();
      if (every) {
        try { pages[j].scrollIntoView({ block: "start" }); } catch (err) { pages[j].scrollIntoView(true); }
      } else show(j, true);
    });
  });
  back.addEventListener("click", function () { show(cur - 1, true); });
  next.addEventListener("click", function () { show(cur + 1, true); });
  all.addEventListener("click", function () { every = !every; show(cur, false); });
  show(0, false);
});
