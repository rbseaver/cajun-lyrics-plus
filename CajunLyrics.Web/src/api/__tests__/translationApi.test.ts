import { translateToEnglish } from '../translationApi';

describe('translateToEnglish', () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('posts the selected text and returns the English translation', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ translation: 'Hello, my friend' }),
    });
    vi.stubGlobal('fetch', fetchMock);

    await expect(translateToEnglish('Bonjour, mon ami')).resolves.toBe('Hello, my friend');
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/Translation/translate',
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ text: 'Bonjour, mon ami' }),
      }),
    );
  });

  it('surfaces an API error instead of returning a fake translation', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: false, status: 503 }));

    await expect(translateToEnglish('Bonjour')).rejects.toThrow('Translation unavailable (503).');
  });
});
