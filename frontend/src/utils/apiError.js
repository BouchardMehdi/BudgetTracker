export function getApiErrorMessage(error) {
  if (!error.response) {
    return "Impossible de joindre l'API. Verifie que le backend est lance.";
  }

  const data = error.response.data;

  if (typeof data === 'string') {
    if (data.trim().startsWith('<')) {
      return `L'API a renvoye une erreur ${error.response.status}. Verifie le routage Docker/Nginx ou les logs du backend.`;
    }

    return data;
  }

  if (data?.message) {
    return data.message;
  }

  if (data?.errors) {
    return Object.values(data.errors).flat().join(' ');
  }

  if (data?.details) {
    return Object.values(data.details).flat().join(' ');
  }

  if (data?.title) {
    return data.title;
  }

  return 'Une erreur est survenue. Reessaie dans un instant.';
}
