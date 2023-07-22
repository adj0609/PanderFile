import React, { useState } from 'react';
import axios from 'axios';
import Dropzone from 'react-dropzone';



const Dashboard = () => {
  const existingClients = [
    { id: 1, name: 'Client A' },
    { id: 2, name: 'Client B' },
    { id: 3, name: 'Client C' },
  ];

  const [selectedClient, setSelectedClient] = useState('');
  const [newClientName, setNewClientName] = useState('');
  const [clients, setClients] = useState(existingClients);
  const [panderFile, setPanderFile]= useState(null);
  const [dataFile, setDataFile] = useState(null);
  const [cleanFile, setCleanFile] = useState(null);
 
  

  const handleSelectChange = (event) => {
    setSelectedClient(event.target.value);
  };

  const handleNewClientChange = (event) => {
    setNewClientName(event.target.value);
  };

  const handleAddNewClient = () => {
    if (newClientName.trim() === '') return;

    const newClient = {
      id: Date.now(),
      name: newClientName,
    };

    setClients([...clients, newClient]);
    setSelectedClient(newClient.id);
    setNewClientName('');
  };

  const handlePanderFile = (acceptedFiles) => {
    setPanderFile(acceptedFiles[0]);
  };

  const handleDataFile = (acceptedFiles) => {
    setDataFile(acceptedFiles[0]);
  };

  const handleFileComparison = async () => {
    if (!selectedClient || !panderFile || !dataFile) {
        alert ('Please select a client and upload both files');
        return;
    }

    const formData = new FormData();
    formData.append('client', selectedClient);
    formData.append('panderFile', panderFile);
    formData.append('dataFile', dataFile);

    try {
        const response = await axios.post('./api/file', formData, {
            headers: { 'Content-Type': 'multipart/form-data'},
        });
        const cleanFileUrl = response.data.cleanFileUrl;
        setCleanFile(cleanFileUrl);
    } catch (error) {
        console.error(error.response.data)
    }
  };

 


  return (
    <div>
      <h2>Select or Add Client</h2>
      <select value={selectedClient} onChange={handleSelectChange}>
        <option value="">Select an existing client</option>
        {clients.map((client) => (
          <option key={client.id} value={client.id}>
            {client.name}
          </option>
        ))}
      </select>
      <br />
      <input
        type="text"
        placeholder="Enter new client name"
        value={newClientName}
        onChange={handleNewClientChange}
      />
      <div>
        <label> Upload your PanderFile </label>
        <input type = "file" onChange={handlePanderFile} />
        {panderFile &&  (
            <p>Pander File: {panderFile.name}</p>
        )}
        <div>
            <label> Upload your Data File</label>
            <input type = 'file' onChange={handleDataFile} />
            {dataFile && (
                <p>Data File: {dataFile.name}</p>
            )}
        </div>
      </div>
      <div>
        <button onClick={handleFileComparison}> Scrub Files</button>
        {cleanFile && (
            <div>
                <p>Processed Clean Files:</p>
                <a href= {cleanFile} target='_blank' rel='noopener noreferrer'>Download Clean File</a>
                </div>
        )}  
      </div>
    </div>     
  );
};

export default Dashboard;
