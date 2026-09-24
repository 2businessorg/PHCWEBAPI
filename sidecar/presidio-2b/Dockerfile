FROM python:3.11-slim

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
# Models are fetched at image build, never on the first /analyze request.
# en_core_web_sm / pt_core_news_sm are the small models (not the large ones).
RUN pip install --no-cache-dir -r requirements.txt \
    && python -m spacy download en_core_web_sm \
    && python -m spacy download pt_core_news_sm

COPY . .

EXPOSE 5001
ENV PORT=5001

CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "5001"]
