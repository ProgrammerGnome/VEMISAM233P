// REST API with ASP.NET Core Web API
// This code defines a simple REST API for a given service (on the backend, this is for a controller).
// The given (unknown) service is referred as "example" with any case.
// A task name (or API purpose) is referred as "task" with any case.
// For example, components/ExampleList.jsx

import React, { useState, useEffect } from 'react';
import { exampleService } from '../services/api';

const ExampleList = () => {
  const [exampleList, setExampleList] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchExampleList = async () => {
      try {
        const response = await exampleService.getExampleList();
        setExampleList(response.data);
      } catch (error) {
        console.error('Error fetching example list:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchExampleList();
  }, []);

  if (loading) return <div>Loading...</div>;

  return (
    <div>
      <h2>Example List</h2>
      {exampleList.map(example => (
        <div key={example.id}>
          <h3>{example.attribute1}</h3>
          <p>{example.attribute2}</p>
        </div>
      ))}
    </div>
  );
};

export default ExampleList;