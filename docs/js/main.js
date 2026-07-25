/* ── Mobile nav toggle ── */
document.querySelector('.nav-toggle')?.addEventListener('click', () => {
  document.querySelector('.nav-links')?.classList.toggle('open');
});

/* ── Close nav on link click ── */
document.querySelectorAll('.nav-links a').forEach(a => {
  a.addEventListener('click', () => {
    document.querySelector('.nav-links')?.classList.remove('open');
  });
});

/* ── Scroll reveal ── */
const revealElements = () => {
  const els = document.querySelectorAll('.feature-card, .source-card, .step, .arch-layer');
  const windowBottom = window.scrollY + window.innerHeight;
  els.forEach(el => {
    if (el.getBoundingClientRect().top + window.scrollY < windowBottom - 60) {
      el.classList.add('visible');
    }
  });
};

window.addEventListener('scroll', revealElements, { passive: true });
window.addEventListener('resize', revealElements, { passive: true });
revealElements(); // initial check
