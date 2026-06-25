using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LanguageReader.Client.Features.Reading.Services;

public sealed class ReaderViewportService(IJSRuntime jsRuntime)
{
    public async Task<string> ObserveScrollAsync<TComponent>(
        ElementReference root,
        DotNetObjectReference<TComponent> dotNetReference)
        where TComponent : class
    {
        return await jsRuntime.InvokeAsync<string>(
            "languageReaderReaderViewport.observeScroll",
            root,
            dotNetReference);
    }

    public async Task UnobserveScrollAsync(string observerId)
    {
        await jsRuntime.InvokeVoidAsync(
            "languageReaderReaderViewport.unobserveScroll",
            observerId);
    }

    public async Task<bool> ScrollBlockIntoViewAsync(
        ElementReference root,
        int blockIndex,
        int offset)
    {
        return await jsRuntime.InvokeAsync<bool>(
            "languageReaderReaderViewport.scrollBlockIntoView",
            root,
            blockIndex,
            offset);
    }

    public async Task<ReaderViewportProgress?> GetProgressAsync(
        ElementReference root,
        Guid readingItemId)
    {
        var snapshot = await jsRuntime.InvokeAsync<ReaderViewportProgressSnapshot?>(
            "languageReaderReaderViewport.getProgress",
            root);

        return snapshot?.BookmarkBlockIndex.HasValue == true
            && snapshot.ProgressBlockIndex.HasValue
            ? new ReaderViewportProgress(
                new ReadingPositionDto(readingItemId, snapshot.BookmarkBlockIndex.Value, 0),
                snapshot.ProgressBlockIndex.Value)
            : null;
    }

    private sealed class ReaderViewportProgressSnapshot
    {
        public int? BookmarkBlockIndex { get; set; }

        public int? ProgressBlockIndex { get; set; }
    }
}
