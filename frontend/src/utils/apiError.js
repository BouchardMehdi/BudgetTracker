export function getApiErrorMessage(error) {
  if (!error.response) {
    return "Impossible de joindre l'API. Verifie que le backend est lance.";
  }

  const data = error.response.data;

  if (typeof data === 'string') {
    return data;
  }

  if (data?.errors) {
    return Object.values(data.errors).flat().join(' ');
  }

  if (data?.title) {
    return data.title;
  }

  return 'Une erreur est survenue. Reessaie dans un instant.';
}
