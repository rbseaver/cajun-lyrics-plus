interface TranslationResponse {
  translation: string;
}

export async function translateToEnglish(text: string): Promise<string> {
  const response = await fetch('/api/Translation/translate', {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ text }),
  });

  if (!response.ok) {
    throw new Error(`Translation unavailable (${response.status}).`);
  }

  const result = (await response.json()) as TranslationResponse;
  return result.translation;
}
