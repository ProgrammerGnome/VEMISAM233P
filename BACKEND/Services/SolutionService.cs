using System.IO;
using ProjectName.Models;
using ProjectName.Models.DTOs;
using ProjectName.Repositories;

namespace ProjectName.Services
{
    public class SolutionService
    {
        private readonly IUploadedSolutionsRepository _repository; 

        public SolutionService(IUploadedSolutionsRepository repository)
        {
            _repository = repository;
        }
        
        public async Task<int> SaveSolutionAsync(SolutionSubmitFormDto dto)
        {
            string codeContent;
            using (var streamReader = new StreamReader(dto.CodeFile.OpenReadStream()))
            {
                codeContent = await streamReader.ReadToEndAsync();
            }
            
            var submissionData = new BekuldottMegoldasContent
            {
                Code = codeContent,
                FileName = dto.CodeFile.FileName,
                EnvironmentDetails = "Uploaded via form data as file." 
            };

            var solution = new FeltoltottMegoldas
            {
                ZhId = dto.ZhId,
                HallgatoId = dto.NeptunKod,
                BekuldottMegoldas = submissionData, 
                Pont = null,
                Ertekeles = null
            };

            await _repository.AddSolutionAsync(solution);
            
            return solution.FeltoltesId;
        }
    }
}