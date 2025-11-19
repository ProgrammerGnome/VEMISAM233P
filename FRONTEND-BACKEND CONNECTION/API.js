// REST API with ASP.NET Core Web API
// This code defines a simple REST API for a given service (on the backend, this is for a controller).
// The given (unknown) service is referred as "example" with any case.
// A task name (or API purpose) is referred as "task" with any case.
// For example, in services/api.js

import axios from 'axios';

const API_BASE_URL = 'https://localhost:7000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const exampleService = {
  getExampleList: () => api.get('/example/list'), // no parameters
  getExampleById: (id) => api.get(`/example/${id}`), // id is passed as a URL parameter
  createExample: (example) => api.post('/example', example), // example data in the request body
  updateExampleById: (id, example) => api.put(`/example/${id}`, example), // id as URL parameter, updated example data in the request body
  deleteExampleById: (id) => api.delete(`/example/${id}`), // id as URL parameter
};