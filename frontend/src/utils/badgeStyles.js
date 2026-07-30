// Utility helper functions for badge color styling across the application

const coachPalette = [
  { bg: 'rgba(99, 102, 241, 0.15)', color: '#6366f1', border: '1px solid rgba(99, 102, 241, 0.3)' }, // Indigo
  { bg: 'rgba(168, 85, 247, 0.15)', color: '#a855f7', border: '1px solid rgba(168, 85, 247, 0.3)' }, // Purple
  { bg: 'rgba(6, 182, 212, 0.15)', color: '#06b6d4', border: '1px solid rgba(6, 182, 212, 0.3)' },   // Cyan
  { bg: 'rgba(244, 63, 94, 0.15)', color: '#f43f5e', border: '1px solid rgba(244, 63, 94, 0.3)' },   // Rose
  { bg: 'rgba(245, 158, 11, 0.15)', color: '#f59e0b', border: '1px solid rgba(245, 158, 11, 0.3)' }, // Amber
  { bg: 'rgba(16, 185, 129, 0.15)', color: '#10b981', border: '1px solid rgba(16, 185, 129, 0.3)' }, // Emerald
  { bg: 'rgba(236, 72, 153, 0.15)', color: '#ec4899', border: '1px solid rgba(236, 72, 153, 0.3)' }, // Pink
  { bg: 'rgba(59, 130, 246, 0.15)', color: '#3b82f6', border: '1px solid rgba(59, 130, 246, 0.3)' }  // Blue
];

const planPalette = [
  { bg: 'rgba(16, 185, 129, 0.15)', color: '#10b981', border: '1px solid rgba(16, 185, 129, 0.3)' }, // Emerald Green
  { bg: 'rgba(245, 158, 11, 0.15)', color: '#f59e0b', border: '1px solid rgba(245, 158, 11, 0.3)' }, // Warm Amber
  { bg: 'rgba(139, 92, 246, 0.15)', color: '#8b5cf6', border: '1px solid rgba(139, 92, 246, 0.3)' }, // Violet
  { bg: 'rgba(59, 130, 246, 0.15)', color: '#3b82f6', border: '1px solid rgba(59, 130, 246, 0.3)' }, // Sky Blue
  { bg: 'rgba(236, 72, 153, 0.15)', color: '#ec4899', border: '1px solid rgba(236, 72, 153, 0.3)' }, // Pink
];

export function getCoachBadgeStyle(coachNombre) {
  if (!coachNombre || coachNombre === 'Sin asignación' || coachNombre === 'Sin coach') {
    return {
      backgroundColor: 'rgba(148, 163, 184, 0.12)',
      color: '#94a3b8',
      border: '1px solid rgba(148, 163, 184, 0.25)',
      padding: '4px 10px',
      borderRadius: '20px',
      fontSize: '12px',
      fontWeight: '600',
      display: 'inline-flex',
      alignItems: 'center',
      gap: '4px'
    };
  }

  // Predefined known coaches
  const nameLower = coachNombre.toLowerCase();
  let style;
  if (nameLower.includes('roberto')) {
    style = coachPalette[0]; // Indigo
  } else if (nameLower.includes('elena')) {
    style = coachPalette[1]; // Purple
  } else if (nameLower.includes('gabriel')) {
    style = coachPalette[2]; // Cyan
  } else if (nameLower.includes('hernán') || nameLower.includes('hernan')) {
    style = coachPalette[3]; // Rose
  } else {
    // Hash based color
    let hash = 0;
    for (let i = 0; i < coachNombre.length; i++) {
      hash = coachNombre.charCodeAt(i) + ((hash << 5) - hash);
    }
    const idx = Math.abs(hash) % coachPalette.length;
    style = coachPalette[idx];
  }

  return {
    backgroundColor: style.bg,
    color: style.color,
    border: style.border,
    padding: '4px 10px',
    borderRadius: '20px',
    fontSize: '12px',
    fontWeight: '600',
    display: 'inline-flex',
    alignItems: 'center',
    gap: '4px'
  };
}

export function getPlanBadgeStyle(planNombre) {
  if (!planNombre || planNombre === 'Sin plan' || planNombre.includes('Sin plan')) {
    return {
      backgroundColor: 'rgba(148, 163, 184, 0.12)',
      color: '#94a3b8',
      border: '1px solid rgba(148, 163, 184, 0.25)',
      padding: '4px 10px',
      borderRadius: '20px',
      fontSize: '12px',
      fontWeight: '600',
      display: 'inline-flex',
      alignItems: 'center',
      gap: '4px'
    };
  }

  const nameLower = planNombre.toLowerCase();
  let style;
  if (nameLower.includes('mensual') || nameLower.includes('full')) {
    style = planPalette[0]; // Emerald Green
  } else if (nameLower.includes('semanal') || nameLower.includes('vip')) {
    style = planPalette[1]; // Warm Amber
  } else if (nameLower.includes('free') || nameLower.includes('libre')) {
    style = planPalette[3]; // Sky Blue
  } else {
    let hash = 0;
    for (let i = 0; i < planNombre.length; i++) {
      hash = planNombre.charCodeAt(i) + ((hash << 5) - hash);
    }
    const idx = Math.abs(hash) % planPalette.length;
    style = planPalette[idx];
  }

  return {
    backgroundColor: style.bg,
    color: style.color,
    border: style.border,
    padding: '4px 10px',
    borderRadius: '20px',
    fontSize: '12px',
    fontWeight: '600',
    display: 'inline-flex',
    alignItems: 'center',
    gap: '4px'
  };
}
